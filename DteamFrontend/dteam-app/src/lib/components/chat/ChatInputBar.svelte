<script lang="ts">
  import { onMount, onDestroy } from 'svelte';
  import { 
    Paperclip, 
    Type, 
    Mic, 
    Send, 
    X, 
    Loader2, 
    FileText, 
    Square,
    Check
  } from 'lucide-svelte';
  import { chatStore } from '../../stores/chatStore';
  import { chatService } from '../../services/chatService';
  import { uiStore } from '../../stores/uiStore';
  import type { ChatUploadResponse } from '../../types/chat';

  interface Props {
    receiverId: string;
    onMessageSent?: () => void;
  }

  let { receiverId, onMessageSent }: Props = $props();

  let textInput = $state('');
  let fileInputElement = $state<HTMLInputElement | null>(null);

  // Attachment state
  let selectedFile = $state<File | null>(null);
  let filePreviewUrl = $state<string | null>(null);
  let isUploading = $state(false);

  // Voice recording state
  let isRecording = $state(false);
  let mediaRecorder: MediaRecorder | null = null;
  let audioChunks: Blob[] = [];
  let recordingSeconds = $state(0);
  let recordingTimerInterval: any = null;

  // Typing debounce timer
  let typingTimeout: any = null;
  let isCurrentlyTyping = false;

  function handleInput(e: Event) {
    const val = (e.target as HTMLInputElement).value;
    textInput = val;

    if (!isCurrentlyTyping && val.trim()) {
      isCurrentlyTyping = true;
      chatService.sendTyping(receiverId, true).catch(() => {});
    }

    if (typingTimeout) clearTimeout(typingTimeout);
    typingTimeout = setTimeout(() => {
      isCurrentlyTyping = false;
      chatService.sendTyping(receiverId, false).catch(() => {});
    }, 1200);
  }

  function handleKeyDown(e: KeyboardEvent) {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      handleSend();
    }
  }

  function handleFileSelect(e: Event) {
    const input = e.target as HTMLInputElement;
    if (input.files && input.files[0]) {
      setFileAttachment(input.files[0]);
    }
  }

  function setFileAttachment(file: File) {
    selectedFile = file;
    if (file.type.startsWith('image/')) {
      filePreviewUrl = URL.createObjectURL(file);
    } else {
      filePreviewUrl = null;
    }
  }

  function clearAttachment() {
    selectedFile = null;
    if (filePreviewUrl) {
      URL.revokeObjectURL(filePreviewUrl);
      filePreviewUrl = null;
    }
    if (fileInputElement) {
      fileInputElement.value = '';
    }
  }

  // Paste image from clipboard
  function handlePaste(e: ClipboardEvent) {
    if (e.clipboardData && e.clipboardData.files && e.clipboardData.files.length > 0) {
      const file = e.clipboardData.files[0];
      if (file.type.startsWith('image/')) {
        e.preventDefault();
        setFileAttachment(file);
      }
    }
  }

  async function handleSend() {
    if (isUploading) return;

    const content = textInput.trim();

    // Case 1: Sending file/image attachment
    if (selectedFile) {
      isUploading = true;
      try {
        const uploadRes = await chatService.uploadAttachment(selectedFile);
        const type = selectedFile.type.startsWith('image/') ? 1 : 2;

        await chatStore.sendMessage(content, type, uploadRes.uploadId, {
          mediaUrl: uploadRes.previewUrl,
          originalFileName: selectedFile.name,
          fileSize: selectedFile.size,
          contentType: selectedFile.type
        });

        textInput = '';
        clearAttachment();
        onMessageSent?.();
      } catch (err: any) {
        console.warn('Upload error:', err);
        uiStore.addToast({
          title: 'Помилка завантаження',
          message: err?.message || 'Не вдалося завантажити файл',
          type: 'error'
        });
      } finally {
        isUploading = false;
      }
      return;
    }

    // Case 2: Plain text message
    if (!content) return;

    const textToSend = content;
    textInput = '';

    if (isCurrentlyTyping) {
      isCurrentlyTyping = false;
      chatService.sendTyping(receiverId, false).catch(() => {});
    }

    await chatStore.sendMessage(textToSend, 0);
    onMessageSent?.();
  }

  // Voice recording logic
  async function startRecording() {
    if (isRecording) return;

    try {
      const stream = await navigator.mediaDevices.getUserMedia({ audio: true });

      // Determine supported MIME type
      let mimeType = 'audio/webm;codecs=opus';
      if (!MediaRecorder.isTypeSupported(mimeType)) {
        if (MediaRecorder.isTypeSupported('audio/webm')) {
          mimeType = 'audio/webm';
        } else if (MediaRecorder.isTypeSupported('audio/ogg;codecs=opus')) {
          mimeType = 'audio/ogg;codecs=opus';
        } else if (MediaRecorder.isTypeSupported('audio/mp4')) {
          mimeType = 'audio/mp4';
        } else {
          mimeType = '';
        }
      }

      mediaRecorder = mimeType ? new MediaRecorder(stream, { mimeType }) : new MediaRecorder(stream);
      audioChunks = [];

      mediaRecorder.ondataavailable = (e) => {
        if (e.data && e.data.size > 0) {
          audioChunks.push(e.data);
        }
      };

      mediaRecorder.start(200);
      isRecording = true;
      recordingSeconds = 0;

      recordingTimerInterval = setInterval(() => {
        recordingSeconds += 1;
        if (recordingSeconds >= 300) { // 5 min limit
          stopAndSendRecording();
        }
      }, 1000);
    } catch (err) {
      console.warn('[Voice] Microphone access error:', err);
      uiStore.addToast({
        title: 'Мікрофон',
        message: 'Не вдалося отримати доступ до мікрофона',
        type: 'error'
      });
    }
  }

  function cancelRecording() {
    if (mediaRecorder && isRecording) {
      mediaRecorder.stop();
      mediaRecorder.stream.getTracks().forEach(t => t.stop());
    }
    clearInterval(recordingTimerInterval);
    isRecording = false;
    audioChunks = [];
    recordingSeconds = 0;
  }

  async function stopAndSendRecording() {
    if (!mediaRecorder || !isRecording) return;

    const duration = recordingSeconds;
    clearInterval(recordingTimerInterval);
    isRecording = false;

    mediaRecorder.onstop = async () => {
      mediaRecorder?.stream.getTracks().forEach(t => t.stop());

      if (audioChunks.length === 0 || duration < 1) {
        return;
      }

      const audioBlob = new Blob(audioChunks, { type: mediaRecorder?.mimeType || 'audio/webm' });
      isUploading = true;

      try {
        const uploadRes = await chatService.uploadAttachment(audioBlob, duration, 'voice_message.webm');
        await chatStore.sendMessage('', 3, uploadRes.uploadId, {
          mediaUrl: uploadRes.previewUrl,
          originalFileName: 'Голосове повідомлення',
          duration,
          fileSize: audioBlob.size,
          contentType: audioBlob.type
        });

        onMessageSent?.();
      } catch (e: any) {
        console.warn('[Voice] Upload error:', e);
        uiStore.addToast({
          title: 'Помилка голосового',
          message: 'Не вдалося надіслати голосове повідомлення',
          type: 'error'
        });
      } finally {
        isUploading = false;
        audioChunks = [];
        recordingSeconds = 0;
      }
    };

    mediaRecorder.stop();
  }

  function formatRecordingTime(sec: number): string {
    const mins = Math.floor(sec / 60);
    const secs = sec % 60;
    return `${mins.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
  }

  onDestroy(() => {
    if (typingTimeout) clearTimeout(typingTimeout);
    if (recordingTimerInterval) clearInterval(recordingTimerInterval);
    if (mediaRecorder && isRecording) {
      mediaRecorder.stop();
      mediaRecorder.stream.getTracks().forEach(t => t.stop());
    }
    if (filePreviewUrl) {
      URL.revokeObjectURL(filePreviewUrl);
    }
  });
</script>

<div class="p-3 sm:p-4 bg-[#061820]/90 border-t border-cyan-500/20 backdrop-blur-xl relative">
  <!-- Attachment Preview Bar (if file selected) -->
  {#if selectedFile}
    <div class="mb-2 p-2.5 rounded-2xl bg-[#09222c] border border-cyan-500/30 flex items-center justify-between gap-3 animate-in slide-in-from-bottom-2">
      <div class="flex items-center gap-3 min-w-0">
        {#if filePreviewUrl}
          <img src={filePreviewUrl} alt="Preview" class="w-12 h-12 rounded-xl object-cover border border-cyan-500/30 shrink-0" />
        {:else}
          <div class="w-10 h-10 rounded-xl bg-cyan-500/20 text-cyan-300 flex items-center justify-center shrink-0">
            <FileText class="w-5 h-5" />
          </div>
        {/if}
        <div class="min-w-0">
          <div class="text-xs font-bold text-white truncate">{selectedFile.name}</div>
          <div class="text-[10px] text-slate-400 font-mono">{(selectedFile.size / 1024).toFixed(1)} КБ</div>
        </div>
      </div>
      <button
        type="button"
        onclick={clearAttachment}
        class="p-1.5 rounded-lg text-slate-400 hover:text-rose-400 hover:bg-slate-800/60 transition-colors cursor-pointer"
        title="Скасувати вкладення"
      >
        <X class="w-4 h-4" />
      </button>
    </div>
  {/if}

  <!-- Main Input Bar / Voice Recorder Container -->
  {#if isRecording}
    <!-- Recording State Bar -->
    <div class="flex items-center justify-between gap-3 px-4 py-2.5 rounded-full bg-[#0d252f] border border-rose-500/40 shadow-[0_0_15px_rgba(244,63,94,0.2)]">
      <div class="flex items-center gap-3">
        <span class="w-3 h-3 rounded-full bg-rose-500 animate-ping"></span>
        <span class="text-xs font-bold text-rose-300 font-mono">
          Запис: {formatRecordingTime(recordingSeconds)}
        </span>
      </div>

      <div class="flex items-center gap-2">
        <!-- Cancel button -->
        <button
          type="button"
          onclick={cancelRecording}
          class="p-2 rounded-full text-slate-400 hover:text-rose-400 hover:bg-slate-800 transition-all cursor-pointer"
          title="Скасувати запис"
        >
          <X class="w-4 h-4" />
        </button>

        <!-- Send voice button -->
        <button
          type="button"
          onclick={stopAndSendRecording}
          class="p-2 rounded-full bg-gradient-to-r from-cyan-400 to-[#0df2c9] text-black shadow-md shadow-cyan-400/30 hover:scale-105 transition-transform cursor-pointer"
          title="Надіслати голосове"
        >
          <Check class="w-4 h-4" />
        </button>
      </div>
    </div>
  {:else}
    <!-- Normal Text / Attachment Input Bar -->
    <div class="flex items-center gap-2 px-3 py-1.5 rounded-full bg-[#081e27] border border-cyan-500/30 focus-within:border-cyan-400 focus-within:shadow-[0_0_15px_rgba(13,242,201,0.25)] transition-all">
      <!-- Hidden File Input -->
      <input
        type="file"
        bind:this={fileInputElement}
        onchange={handleFileSelect}
        class="hidden"
      />

      <!-- Format Button (T) -->
      <button
        type="button"
        class="p-2 rounded-full text-slate-400 hover:text-cyan-300 hover:bg-slate-800/40 transition-colors cursor-pointer shrink-0"
        title="Форматування"
      >
        <Type class="w-4 h-4" />
      </button>

      <!-- Attach Button (Clip) -->
      <button
        type="button"
        onclick={() => fileInputElement?.click()}
        class="p-2 rounded-full text-slate-400 hover:text-cyan-300 hover:bg-slate-800/40 transition-colors cursor-pointer shrink-0"
        title="Прикріпити фото або файл"
      >
        <Paperclip class="w-4 h-4" />
      </button>

      <!-- Text Input Field -->
      <input
        type="text"
        placeholder={selectedFile ? 'Додайте підпис до файлу...' : 'Ваше повідомлення...'}
        value={textInput}
        oninput={handleInput}
        onkeydown={handleKeyDown}
        onpaste={handlePaste}
        class="flex-1 bg-transparent border-none text-xs text-white placeholder-slate-400/80 focus:outline-none px-2 py-2"
      />

      <!-- Send or Mic Button -->
      {#if isUploading}
        <div class="p-2 text-cyan-400 animate-spin shrink-0">
          <Loader2 class="w-4 h-4" />
        </div>
      {:else if textInput.trim() || selectedFile}
        <button
          type="button"
          onclick={handleSend}
          class="p-2 rounded-full bg-gradient-to-r from-cyan-400 to-[#0df2c9] text-black shadow-md shadow-cyan-400/30 hover:scale-105 transition-transform cursor-pointer shrink-0"
          title="Надіслати"
        >
          <Send class="w-4 h-4 translate-x-0.5" />
        </button>
      {:else}
        <button
          type="button"
          onclick={startRecording}
          class="p-2 rounded-full text-slate-400 hover:text-cyan-300 hover:bg-slate-800/40 transition-colors cursor-pointer shrink-0"
          title="Записати голосове повідомлення"
        >
          <Mic class="w-4 h-4" />
        </button>
      {/if}
    </div>
  {/if}
</div>

import { api } from './api';

export interface UploadMediaResponse {
  url: string;
  fileName: string;
  size: number;
  type: 'image' | 'video';
  message: string;
}

export const ALLOWED_IMAGE_TYPES = ['image/jpeg', 'image/png', 'image/webp', 'image/gif'];
export const ALLOWED_VIDEO_TYPES = ['video/mp4', 'video/webm', 'video/quicktime'];

export const MAX_IMAGE_SIZE_BYTES = 20 * 1024 * 1024; // 20 МБ
export const MAX_VIDEO_SIZE_BYTES = 150 * 1024 * 1024; // 150 МБ

export const mediaService = {
  // Завантажує зображення або відео на сервер і повертає публічний URL файлу.
  async upload(file: File): Promise<UploadMediaResponse> {
    const formData = new FormData();
    formData.append('file', file);
    return await api.post<UploadMediaResponse>('/media/upload', formData);
  },

  // Генерує прев'ю (кадр) з відеофайлу прямо в браузері та завантажує його як картинку.
  // Повертає URL згенерованого прев'ю, який можна використати як thumbnailUrl поста.
  async generateAndUploadVideoThumbnail(file: File): Promise<string> {
    const objectUrl = URL.createObjectURL(file);
    try {
      const dataUrl = await new Promise<string>((resolve, reject) => {
        const video = document.createElement('video');
        video.preload = 'metadata';
        video.muted = true;
        video.playsInline = true;
        video.src = objectUrl;

        video.onloadeddata = () => {
          try {
            video.currentTime = Math.min(1, (video.duration || 1) / 2);
          } catch {
            video.currentTime = 0;
          }
        };

        video.onseeked = () => {
          try {
            const canvas = document.createElement('canvas');
            canvas.width = video.videoWidth || 640;
            canvas.height = video.videoHeight || 360;
            const ctx = canvas.getContext('2d');
            if (!ctx) {
              reject(new Error('Canvas недоступний.'));
              return;
            }
            ctx.drawImage(video, 0, 0, canvas.width, canvas.height);
            resolve(canvas.toDataURL('image/jpeg', 0.85));
          } catch (err) {
            reject(err);
          }
        };

        video.onerror = () => reject(new Error('Не вдалося обробити відеофайл для створення прев\'ю.'));
      });

      const blob = await (await fetch(dataUrl)).blob();
      const thumbFile = new File([blob], 'thumbnail.jpg', { type: 'image/jpeg' });
      const uploaded = await mediaService.upload(thumbFile);
      return uploaded.url;
    } finally {
      URL.revokeObjectURL(objectUrl);
    }
  },
};

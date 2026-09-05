import { api } from './api';
import { communityService } from './communityService';

export interface UploadMediaResponse {
  url: string;
  fileName: string;
  size: number;
  type: 'image' | 'video';
  message: string;
}

export const ALLOWED_IMAGE_TYPES = ['image/jpeg', 'image/png', 'image/webp', 'image/gif', 'image/pjpeg', 'image/x-png'];
export const ALLOWED_VIDEO_TYPES = ['video/mp4', 'video/webm', 'video/quicktime', 'video/x-matroska', 'video/avi'];

export const MAX_IMAGE_SIZE_BYTES = 20 * 1024 * 1024;
export const MAX_VIDEO_SIZE_BYTES = 150 * 1024 * 1024;

export const mediaService = {
  async upload(file: File): Promise<UploadMediaResponse> {
    try {
      const res = await communityService.uploadMedia(file);
      return {
        url: res.url,
        fileName: res.fileName,
        size: file.size,
        type: res.type,
        message: 'Файл успішно завантажено'
      };
    } catch {
      const formData = new FormData();
      formData.append('file', file);
      return await api.post<UploadMediaResponse>('/media/upload', formData);
    }
  },

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
      try {
        const uploaded = await communityService.uploadMedia(thumbFile);
        return uploaded.url;
      } catch {
        const uploaded = await mediaService.upload(thumbFile);
        return uploaded.url;
      }
    } finally {
      URL.revokeObjectURL(objectUrl);
    }
  },
};


import { defineConfig } from 'vite'
import { svelte } from '@sveltejs/vite-plugin-svelte'
import tailwindcss from '@tailwindcss/vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [
    svelte(),
    tailwindcss(),
  ],
  server: {
    port: 5173,
    proxy: {
      '/api': {
        target: 'http://localhost:5117',
        changeOrigin: true,
        secure: false,
      },
      '/hubs': {
        target: 'http://localhost:5117',
        changeOrigin: true,
        secure: false,
        ws: true,
      },
      '/hub': {
        target: 'http://localhost:5117',
        changeOrigin: true,
        secure: false,
        ws: true,
      },
      '/game_images': {
        target: 'http://localhost:5117',
        changeOrigin: true,
        secure: false,
      }
    }
  }
})

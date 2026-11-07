import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

// In development the React dev server proxies API calls to the .NET app, so both run on one origin
// just like production, where the .NET app serves the built files from its wwwroot.
const apiTarget = process.env.API_URL ?? 'http://localhost:5292';

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      '/api': apiTarget,
      '/uploads': apiTarget
    }
  },
  build: {
    outDir: '../MinnieWorld.Web/wwwroot',
    emptyOutDir: true
  }
});

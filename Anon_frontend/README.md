# Anon_frontend

React + TypeScript + Vite Application

## Prerequisites

- Node.js 18+

## Quick Start

### 1. Install Dependencies

```bash
npm install
```

### 2. Start Development Server

```bash
npm run dev
```

Frontend runs at https://localhost:5173

### 3. Build with Docker

```bash
docker-compose up --build
```

Runs at http://localhost:3000

## Notes

- This frontend expects a backend running for API calls.
- In development (`npm run dev`), it proxies `/api` to `https://localhost:5000`.
- In production (Docker/Nginx), it attempts to proxy `/api` to `http://backend:8080`.

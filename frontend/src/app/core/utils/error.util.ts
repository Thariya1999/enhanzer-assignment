import { HttpErrorResponse } from '@angular/common/http';

interface ApiErrorBody {
  message?: string;
  title?: string;
  errors?: Record<string, string[]>;
}

/** Turns any HTTP / unknown error into a user friendly message. */
export function extractErrorMessage(err: unknown, fallback = 'Something went wrong. Please try again.'): string {
  if (err instanceof HttpErrorResponse) {
    if (err.status === 0) {
      return 'Cannot reach the server. Please make sure the API is running.';
    }
    const body = (err.error ?? {}) as ApiErrorBody;
    if (body.message) return body.message;
    if (body.errors) {
      const first = Object.values(body.errors).flat()[0];
      if (first) return first;
    }
    if (body.title) return body.title;
  }
  return fallback;
}

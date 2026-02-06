import { Injectable, signal } from '@angular/core';

export interface Toast {
  id: number;
  message: string;
  type: 'success' | 'error';
  fadeOut?: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class ToastService {
  private nextId = 0;
  toasts = signal<Toast[]>([]);

  show(message: string, type: 'success' | 'error' = 'success', duration = 3000): void {
    const id = this.nextId++;
    const toast: Toast = { id, message, type };

    this.toasts.update(toasts => [...toasts, toast]);

    setTimeout(() => {
      this.toasts.update(toasts =>
        toasts.map(t => t.id === id ? { ...t, fadeOut: true } : t)
      );

      setTimeout(() => {
        this.toasts.update(toasts => toasts.filter(t => t.id !== id));
      }, 300);
    }, duration);
  }

  success(message: string): void {
    this.show(message, 'success');
  }

  error(message: string): void {
    this.show(message, 'error', 5000);
  }
}

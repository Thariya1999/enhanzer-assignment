import { Component } from '@angular/core';

@Component({
  selector: 'app-spinner',
  template: '<span class="spinner" role="status" aria-label="Loading"></span>',
  styles: `
    .spinner {
      display: inline-block;
      width: 1em;
      height: 1em;
      border: 2px solid currentColor;
      border-right-color: transparent;
      border-radius: 50%;
      animation: spin 0.7s linear infinite;
      vertical-align: -0.15em;
    }
    @keyframes spin {
      to { transform: rotate(360deg); }
    }
  `,
})
export class SpinnerComponent {}

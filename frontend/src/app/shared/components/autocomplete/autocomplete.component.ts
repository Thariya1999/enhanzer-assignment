import { Component, computed, input, signal } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';

/** Reusable autocomplete bound to a FormControl. Suggestions filter as you type. */
@Component({
  selector: 'app-autocomplete',
  imports: [ReactiveFormsModule],
  templateUrl: './autocomplete.component.html',
  styleUrl: './autocomplete.component.scss',
})
export class AutocompleteComponent {
  readonly label = input.required<string>();
  readonly options = input.required<readonly string[]>();
  readonly control = input.required<FormControl<string | null>>();
  readonly placeholder = input('');
  readonly inputId = input('autocomplete');

  protected readonly open = signal(false);
  protected readonly text = signal('');
  protected readonly activeIndex = signal(-1);

  protected readonly filtered = computed(() => {
    const q = this.text().trim().toLowerCase();
    return q ? this.options().filter((o) => o.toLowerCase().includes(q)) : [...this.options()];
  });

  protected get showError(): boolean {
    const c = this.control();
    return c.invalid && (c.touched || c.dirty);
  }

  protected get errorText(): string {
    const errors = this.control().errors;
    if (errors?.['required']) return `${this.label()} is required`;
    if (errors?.['notInList']) return 'Please pick a value from the list';
    return 'Invalid value';
  }

  protected onFocus(): void {
    this.text.set(this.control().value ?? '');
    this.activeIndex.set(-1);
    this.open.set(true);
  }

  protected onInput(event: Event): void {
    this.text.set((event.target as HTMLInputElement).value);
    this.activeIndex.set(-1);
    this.open.set(true);
  }

  protected onBlur(): void {
    this.open.set(false);
    this.control().markAsTouched();
  }

  protected select(option: string): void {
    const c = this.control();
    c.setValue(option);
    c.markAsDirty();
    this.text.set(option);
    this.open.set(false);
  }

  protected onKeydown(event: KeyboardEvent): void {
    const list = this.filtered();
    switch (event.key) {
      case 'ArrowDown':
        event.preventDefault();
        this.open.set(true);
        this.activeIndex.set(Math.min(this.activeIndex() + 1, list.length - 1));
        break;
      case 'ArrowUp':
        event.preventDefault();
        this.activeIndex.set(Math.max(this.activeIndex() - 1, 0));
        break;
      case 'Enter':
        if (this.open() && this.activeIndex() >= 0) {
          event.preventDefault();
          this.select(list[this.activeIndex()]);
        }
        break;
      case 'Escape':
        this.open.set(false);
        break;
    }
  }
}

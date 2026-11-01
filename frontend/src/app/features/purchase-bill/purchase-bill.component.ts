import { DecimalPipe } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { startWith } from 'rxjs';
import { Location } from '../../core/models/auth.models';
import { BillItem } from '../../core/models/purchase-bill.models';
import { AuthService } from '../../core/services/auth.service';
import { LocationService } from '../../core/services/location.service';
import { PurchaseBillService } from '../../core/services/purchase-bill.service';
import { calcMargin, calcTotalCost, calcTotalSelling, round2 } from '../../core/utils/calculations';
import { extractErrorMessage } from '../../core/utils/error.util';
import { AutocompleteComponent } from '../../shared/components/autocomplete/autocomplete.component';
import { SpinnerComponent } from '../../shared/components/spinner/spinner.component';

export const ITEM_OPTIONS = ['Mango', 'Apple', 'Banana', 'Orange', 'Grapes', 'Kiwi', 'Strawberry'] as const;

/** The typed text must be exactly one of the allowed values. */
const inList = (allowed: readonly string[]): ValidatorFn => (c: AbstractControl): ValidationErrors | null =>
  !c.value || allowed.some((a) => a.toLowerCase() === String(c.value).toLowerCase()) ? null : { notInList: true };

@Component({
  selector: 'app-purchase-bill',
  imports: [ReactiveFormsModule, DecimalPipe, AutocompleteComponent, SpinnerComponent],
  templateUrl: './purchase-bill.component.html',
  styleUrl: './purchase-bill.component.scss',
})
export class PurchaseBillComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly locationService = inject(LocationService);
  private readonly billService = inject(PurchaseBillService);

  protected readonly itemOptions = ITEM_OPTIONS;
  protected readonly userEmail = computed(() => this.auth.session()?.email ?? '');

  protected readonly form = this.fb.group({
    item: ['', [Validators.required, inList(ITEM_OPTIONS)]],
    batch: ['', [Validators.required]],
    standardCost: [null as number | null, [Validators.required, Validators.min(0)]],
    standardPrice: [null as number | null, [Validators.required, Validators.min(0)]],
    qty: [null as number | null, [Validators.required, Validators.min(1)]],
    freeQty: [0 as number | null, [Validators.min(0)]],
    discount: [0 as number | null, [Validators.min(0), Validators.max(100)]],
  });

  // ----- state -----
  protected readonly batches = signal<Location[]>([]);
  protected readonly loadingBatches = signal(false);
  protected readonly batchError = signal('');
  protected readonly rows = signal<BillItem[]>([]);
  protected readonly saving = signal(false);
  protected readonly saveError = signal('');
  protected readonly saveSuccess = signal('');

  // ----- live calculations for the entry row -----
  private readonly values = toSignal(this.form.valueChanges.pipe(startWith(this.form.getRawValue())), {
    initialValue: this.form.getRawValue(),
  });
  protected readonly margin = computed(() => calcMargin(this.values().standardCost ?? 0, this.values().standardPrice ?? 0));
  protected readonly totalCost = computed(() => {
    const v = this.values();
    return calcTotalCost(v.standardCost ?? 0, v.qty ?? 0, v.discount ?? 0);
  });
  protected readonly totalSelling = computed(() => {
    const v = this.values();
    return calcTotalSelling(v.standardPrice ?? 0, v.qty ?? 0);
  });

  // ----- summary panel -----
  protected readonly totalItems = computed(() => this.rows().length);
  protected readonly totalQty = computed(() => this.rows().reduce((sum, r) => sum + r.qty, 0));
  protected readonly grossTotal = computed(() => round2(this.rows().reduce((s, r) => s + r.standardCost * r.qty, 0)));
  protected readonly totalDiscounts = computed(() =>
    round2(this.rows().reduce((s, r) => s + (r.standardCost * r.qty * r.discountPercent) / 100, 0)),
  );
  protected readonly netCost = computed(() => round2(this.rows().reduce((s, r) => s + r.totalCost, 0)));
  protected readonly netSelling = computed(() => round2(this.rows().reduce((s, r) => s + r.totalSelling, 0)));

  ngOnInit(): void {
    this.loadBatches();
  }

  protected loadBatches(): void {
    this.loadingBatches.set(true);
    this.batchError.set('');
    this.locationService.getLocations().subscribe({
      next: (list) => {
        this.batches.set(list);
        this.loadingBatches.set(false);
      },
      error: (err: unknown) => {
        this.batchError.set(extractErrorMessage(err, 'Could not load batches.'));
        this.loadingBatches.set(false);
      },
    });
  }

  protected invalid(name: keyof typeof this.form.controls): boolean {
    const c = this.form.controls[name];
    return c.invalid && (c.touched || c.dirty);
  }

  protected add(): void {
    this.saveSuccess.set('');
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const v = this.form.getRawValue();
    const cost = v.standardCost ?? 0;
    const price = v.standardPrice ?? 0;
    const qty = v.qty ?? 0;
    const discount = v.discount ?? 0;

    const row: BillItem = {
      item: ITEM_OPTIONS.find((o) => o.toLowerCase() === (v.item ?? '').toLowerCase()) ?? (v.item ?? ''),
      batch: v.batch ?? '',
      standardCost: cost,
      standardPrice: price,
      margin: calcMargin(cost, price),
      qty,
      freeQty: v.freeQty ?? 0,
      discountPercent: discount,
      totalCost: calcTotalCost(cost, qty, discount),
      totalSelling: calcTotalSelling(price, qty),
    };
    this.rows.update((r) => [...r, row]);
    this.form.reset({ item: '', batch: v.batch, standardCost: null, standardPrice: null, qty: null, freeQty: 0, discount: 0 });
  }

  protected remove(index: number): void {
    this.rows.update((r) => r.filter((_, i) => i !== index));
  }

  protected save(): void {
    if (!this.rows().length) return;
    this.saving.set(true);
    this.saveError.set('');
    this.saveSuccess.set('');
    this.billService.save(this.rows()).subscribe({
      next: (res) => {
        this.saving.set(false);
        this.saveSuccess.set(`Purchase bill #${res.id} saved (${res.totalItems} items, qty ${res.totalQty}).`);
        this.rows.set([]);
      },
      error: (err: unknown) => {
        this.saving.set(false);
        this.saveError.set(extractErrorMessage(err, 'Could not save the purchase bill.'));
      },
    });
  }

  protected logout(): void {
    this.auth.logout();
  }
}

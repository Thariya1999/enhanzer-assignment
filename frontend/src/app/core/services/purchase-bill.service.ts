import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { BillItem, SavePurchaseBillRequest, SavePurchaseBillResponse } from '../models/purchase-bill.models';

@Injectable({ providedIn: 'root' })
export class PurchaseBillService {
  private readonly http = inject(HttpClient);

  save(items: BillItem[]): Observable<SavePurchaseBillResponse> {
    const body: SavePurchaseBillRequest = { items };
    return this.http.post<SavePurchaseBillResponse>('/api/purchase-bills', body);
  }
}

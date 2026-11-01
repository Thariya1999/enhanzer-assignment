export interface BillItem {
  item: string;
  batch: string;
  standardCost: number;
  standardPrice: number;
  margin: number;
  qty: number;
  freeQty: number;
  discountPercent: number;
  totalCost: number;
  totalSelling: number;
}

export interface SavePurchaseBillRequest {
  items: BillItem[];
}

export interface SavePurchaseBillResponse {
  id: number;
  totalItems: number;
  totalQty: number;
  totalCost: number;
  totalSelling: number;
}

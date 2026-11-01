export const round2 = (n: number): number => Math.round((n + Number.EPSILON) * 100) / 100;

/** Total Cost = (Standard Cost x Qty) less Discount %.  e.g. 100 x 5 - 20% = 400 */
export const calcTotalCost = (standardCost: number, qty: number, discountPercent: number): number =>
  round2(standardCost * qty * (1 - discountPercent / 100));

/** Total Selling = Standard Price x Qty.  e.g. 150 x 5 = 750 */
export const calcTotalSelling = (standardPrice: number, qty: number): number => round2(standardPrice * qty);

/** Margin = Standard Price - Standard Cost (per unit) */
export const calcMargin = (standardCost: number, standardPrice: number): number =>
  round2(standardPrice - standardCost);

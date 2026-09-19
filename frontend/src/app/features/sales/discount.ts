export const MAX_QUANTITY = 20;

/** Mirrors the API's DiscountPolicy for instant feedback in the form. Null means the quantity is not allowed. */
export function discountPercentFor(quantity: number): number | null {
  if (!Number.isInteger(quantity) || quantity < 1 || quantity > MAX_QUANTITY) return null;
  if (quantity >= 10) return 0.2;
  if (quantity >= 4) return 0.1;
  return 0;
}

export function previewItem(unitPrice: number, quantity: number): { percent: number; discount: number; total: number } | null {
  const percent = discountPercentFor(quantity);
  if (percent === null || !(unitPrice > 0)) return null;
  const gross = unitPrice * quantity;
  const discount = Math.round(gross * percent * 100) / 100;
  return { percent, discount, total: gross - discount };
}

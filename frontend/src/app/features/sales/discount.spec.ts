import { discountPercentFor, previewItem } from './discount';

describe('discount tiers (UI preview only; the API is authoritative)', () => {
  it('follows the quantity tiers', () => {
    expect(discountPercentFor(1)).toBe(0);
    expect(discountPercentFor(3)).toBe(0);
    expect(discountPercentFor(4)).toBe(0.1);
    expect(discountPercentFor(9)).toBe(0.1);
    expect(discountPercentFor(10)).toBe(0.2);
    expect(discountPercentFor(20)).toBe(0.2);
  });

  it('returns null above the 20-unit cap', () => {
    expect(discountPercentFor(21)).toBeNull();
    expect(discountPercentFor(0)).toBeNull();
  });

  it('previews line totals', () => {
    expect(previewItem(10, 5)).toEqual({ percent: 0.1, discount: 5, total: 45 });
    expect(previewItem(10, 21)).toBeNull();
  });
});

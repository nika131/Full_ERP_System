export function formatCurrency(value: number): string {
  return `$${value.toFixed(2)}`;
}

export function round2(value: number): number {
  return Math.round((value + Number.EPSILON) * 100) / 100;
}

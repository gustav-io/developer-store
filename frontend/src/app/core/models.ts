export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
}

export interface PaginatedResponse<T> extends ApiResponse<T[]> {
  currentPage: number;
  totalPages: number;
  totalCount: number;
}

export interface ApiError {
  type: string;
  error: string;
  detail: string;
}

export type SaleStatus = 'Active' | 'Cancelled';

export interface SaleItem {
  id: string;
  productId: string;
  productName: string;
  unitPrice: number;
  quantity: number;
  discountPercent: number;
  discountAmount: number;
  totalAmount: number;
  isCancelled: boolean;
}

export interface Sale {
  id: string;
  saleNumber: number;
  saleDate: string;
  customerId: string;
  customerName: string;
  branchId: string;
  branchName: string;
  status: SaleStatus;
  totalAmount: number;
  createdAt: string;
  updatedAt: string | null;
  items: SaleItem[];
}

export interface SaleItemInput {
  productId: string;
  productName: string;
  unitPrice: number;
  quantity: number;
}

export interface SaleInput {
  saleDate?: string;
  customerId: string;
  customerName: string;
  branchId: string;
  branchName: string;
  items: SaleItemInput[];
}

export interface ListSalesQuery {
  page: number;
  size: number;
  order?: string;
  customerName?: string;
  status?: SaleStatus | '';
}

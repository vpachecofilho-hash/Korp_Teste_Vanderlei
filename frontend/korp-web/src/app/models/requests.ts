export interface CreateProductRequest {
  code: string;
  description: string;
  stock: number;
}

export interface CreateInvoiceItemRequest {
  productId: string;
  quantity: number;
}

export interface CreateInvoiceRequest {
  items: CreateInvoiceItemRequest[];
}
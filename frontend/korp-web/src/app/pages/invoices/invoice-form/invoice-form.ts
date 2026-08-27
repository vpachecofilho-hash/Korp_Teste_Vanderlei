import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';

import { Product } from '../../../models/product';
import { ProductService } from '../../../core/services/product.service';
import { InvoiceService } from '../../../core/services/invoice.service';

@Component({
  selector: 'app-invoice-form',
  imports: [
    CommonModule,
    FormsModule
  ],
  templateUrl: './invoice-form.html',
  styleUrl: './invoice-form.scss'
})
export class InvoiceForm implements OnInit {

  products: Product[] = [];

  selectedProductId = '';
  quantity = 1;

  items: {
    productId: string;
    quantity: number;
  }[] = [];

  loading = false;
  errorMessage = '';

  constructor(
    private productService: ProductService,
    private invoiceService: InvoiceService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadProducts();
  }

  loadProducts(): void {
    this.productService.getAll().subscribe({
      next: products => {
        this.products = products;
        this.cdr.detectChanges();
      },
      error: () => {
        this.errorMessage = 'Não foi possível carregar os produtos.';
      }
    });
  }

  addItem(): void {
    if (!this.selectedProductId || this.quantity <= 0) {
      return;
    }

    const alreadyAdded = this.items
      .some(item => item.productId === this.selectedProductId);

    if (alreadyAdded) {
      this.errorMessage = 'Este produto já foi adicionado à nota.';
      this.cdr.detectChanges();
      return;
    }

    this.items.push({
      productId: this.selectedProductId,
      quantity: this.quantity
    });

    this.selectedProductId = '';
    this.quantity = 1;
    this.errorMessage = '';

    this.cdr.detectChanges();
  }

  removeItem(productId: string): void {
    this.items = this.items
      .filter(item => item.productId !== productId);

    this.cdr.detectChanges();
  }

  getProductDescription(productId: string): string {
    return this.products
      .find(product => product.id === productId)
      ?.description ?? productId;
  }

  save(): void {
    if (this.items.length === 0) {
      this.errorMessage = 'Adicione pelo menos um produto.';
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    this.invoiceService.create({
      items: this.items
    }).subscribe({
      next: invoice => {
        this.loading = false;
        this.cdr.detectChanges();

        this.router.navigate([
          '/invoices',
          invoice.id
        ]);
      },

      error: error => {
        this.loading = false;

        this.errorMessage =
          error.error?.message ??
          'Não foi possível criar a nota.';

        this.cdr.detectChanges();
      }
    });
  }
}
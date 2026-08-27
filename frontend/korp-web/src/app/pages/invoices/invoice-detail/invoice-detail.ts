import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { finalize } from 'rxjs';

import { Invoice } from '../../../models/invoice';
import { Product } from '../../../models/product';

import { InvoiceService }
  from '../../../core/services/invoice.service';

import { ProductService }
  from '../../../core/services/product.service';


@Component({
  selector: 'app-invoice-detail',
  imports: [
    CommonModule
  ],
  templateUrl: './invoice-detail.html',
  styleUrl: './invoice-detail.scss'
})
export class InvoiceDetail implements OnInit {

  invoice?: Invoice;

  products: Product[] = [];

  loading = false;
  processing = false;

  errorMessage = '';

  constructor(
    private route: ActivatedRoute,
    private invoiceService: InvoiceService,
    private productService: ProductService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadProducts();
    this.loadInvoice();
  }

  loadProducts(): void {
    this.productService.getAll()
      .subscribe({
        next: products => {
          this.products = products;
        }
      });
  }

  loadInvoice(): void {
    const id = this.route.snapshot.paramMap.get('id');

    if (!id) {
      this.errorMessage = 'Nota inválida.';
      return;
    }

    this.loading = true;

    this.invoiceService.getById(id)
      .pipe(
        finalize(() => {
          this.loading = false;
          this.cdr.detectChanges();
        })
      )
      .subscribe({
        next: invoice => {
          this.invoice = invoice;
        },

        error: error => {
          console.error(error);

          this.errorMessage =
            'Não foi possível carregar as notas.';
        }
      });
  }

  getProductDescription(productId: string): string {
    const product = this.products
      .find(product => product.id === productId);

    if (!product) {
      return productId;
    }

    return `${product.code} - ${product.description}`;
  }

  closeInvoice(): void {
    if (!this.invoice) {
      return;
    }

    this.processing = true;
    this.errorMessage = '';

    this.invoiceService
      .close(this.invoice.id)
      .pipe(
        finalize(() => {
          this.processing = false;
          this.cdr.detectChanges();
        })
      )
      .subscribe({
        next: invoice => {
          this.invoice = invoice;
          this.cdr.detectChanges();

          setTimeout(() => {
            window.print();
          }, 100);
        },

        error: error => {
          this.errorMessage =
            error.error?.message ??
            'Não foi possível imprimir a nota.';

          this.cdr.detectChanges();
        }
      });
  }
}
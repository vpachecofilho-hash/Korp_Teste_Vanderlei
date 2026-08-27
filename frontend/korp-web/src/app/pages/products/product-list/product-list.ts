import { ChangeDetectorRef,Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ProductService } from '../../../core/services/product.service';
import { Product } from '../../../models/product';
import { finalize } from 'rxjs';


@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './product-list.html'
})
export class ProductList implements OnInit {

  products: Product[] = [];
  loading = false;
  errorMessage = '';

  constructor(private productService: ProductService,
  private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.loadProducts();
  }

  loadProducts(): void {
    this.loading = true;
    this.errorMessage = '';

    this.productService.getAll()
      .pipe(
        finalize(() => {
          this.loading = false;
          this.cdr.detectChanges();
        })
      )
      .subscribe({
        next: products => {
          this.products = products;
        },

        error: error => {
          console.error(error);

          this.errorMessage =
            'Não foi possível carregar os produtos.';
        }
      });
  }
}
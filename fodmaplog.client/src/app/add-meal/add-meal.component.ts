import { Component } from '@angular/core';
import { Product } from '../domain/Product';
import { ProductsApiService } from '../services/product-api.service';

@Component({
  selector: 'app-add-meal',
  templateUrl: './add-meal.component.html',
  styleUrl: './add-meal.component.css'
})
export class AddMealComponent {

  constructor(private productsApiService: ProductsApiService) { }

  products: Product[];

GetProduct(): Product {
    return this.productsApiService.GetProduct().subscribe(results => this.products = results);
  }
}

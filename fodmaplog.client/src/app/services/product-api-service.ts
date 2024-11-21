import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { Product } from "../domain/Product";

export class ProductApiService{
    constructor(private httpClient: HttpClient) { }

    GetProductsByKeyword(keyword: string): Observable<Product[]> {
        return this.httpClient.get<Product[]>(`https://api.example.com/products?keyword=${keyword}`);
    }
}
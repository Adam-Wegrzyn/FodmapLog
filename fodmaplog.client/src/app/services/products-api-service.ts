import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { Product } from "../domain/Product";
import { Injectable } from "@angular/core";

@Injectable({
    providedIn: 'root'
})
export class ProductsApiService{
    url: string = "https://localhost:44349/api/ProductsApi"
    constructor(private httpClient: HttpClient) { }

    getProductsByKeyword(keyword: string): Observable<Product[]> {
        return this.httpClient.get<Product[]>(`${this.url}/getProductsByKeyword/${keyword}`);
    }
}
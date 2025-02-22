import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { Product } from "../domain/Product";
import { Injectable } from "@angular/core";
import { environment } from "../../environments/environment.prod";

@Injectable({
    providedIn: 'root'
})
export class ProductsApiService{
    url = environment.apiOpenFood;
    constructor(private httpClient: HttpClient) { }

    getProductsByKeyword(keyword: string): Observable<Product[]> {
        return this.httpClient.get<Product[]>(`${this.url}/getProductsByKeyword/${keyword}`);
    }
}
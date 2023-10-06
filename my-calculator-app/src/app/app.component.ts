import { Component } from '@angular/core';
import {HttpClient} from "@angular/common/http";

export interface requestBody {
  Number1: number,
  Number2: number
}

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {
  title = 'my-calculator-app';

  constructor(private http: HttpClient) {
  }

  public onAdditionClick(requestBody: requestBody) {
    const additionEndpoint = '/addition/';
    this.http.post(additionEndpoint, requestBody).subscribe(
      (response) => {
        console.log(response)
      },
      (error) => {
        console.error(error); // Log the error
      }
    );
  }

  public onSubtractionClick(requestBody: requestBody) {
    const additionEndpoint = '/subtraction/';
    this.http.post(additionEndpoint, requestBody).subscribe(
      (response) => {
        console.log(response)
      },
      (error) => {
        console.error(error); // Log the error
      }
    );
  }
}


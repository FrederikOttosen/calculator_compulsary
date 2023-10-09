import {Component, signal} from '@angular/core';
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
  count = signal<string>('');
  tempCount = signal<string>('');
  calculationCount = signal<number>(0);

  constructor(
    private http: HttpClient) {
  }

  onSignChangeClick() {
    let currentVal = this.count();
    // Check if the current value is negative.
    if (currentVal.startsWith('-')) {
      // Remove the '-' sign to make it positive.
      this.count.set(currentVal.slice(1));
    } else {
      // Add a '-' sign to make it negative.
      this.count.set('-' + currentVal);
    }
  }

  onClickClearCount() {
    this.count.set('');
  }

  onClickClearAll() {
    this.count.set('');
    this.tempCount.set('');
    this.calculationCount.set(0);
  }

  onOperationClick(operator: string) {
    this.tempCount.set(this.tempCount() + ' ' + this.count() + ' ' + operator)
    this.count.set('');
  }

  onNumberClick(number: number) {
    this.count.set(this.count() + number.toString());
  }

  parseOperations(finalString: string) {
    // Split the string based on spaces and then filter out any empty strings
    const parts = finalString.split(' ').filter(Boolean);

    // Create an array to hold operations and their corresponding operands
    const operations: any = [];

    let currentOperator: string | null = null;

    parts.forEach((part, index) => {
      if (['+', '-', '*', '/'].includes(part)) {
        // Store the current operator
        currentOperator = part;
      } else {
        // This is a number
        const operandObj: { operand: number, operator?: string } = {
          operand: parseFloat(part)
        };

        if (currentOperator) {
          operandObj.operator = currentOperator;
          currentOperator = null;
        }

        operations.push(operandObj);
      }
    });

    if (currentOperator !== null) {
      throw new Error("Unexpected sequence in finalString. Operator without operand.");
    }

    return operations;
  }

  public onSubtractionClick(requestBody: requestBody) {
    const subtractionEndpoint = '/subtraction/';
    this.http.post(subtractionEndpoint, requestBody).subscribe(
      (response: any) => {
        const resultElement = document.getElementById("input");
        if (resultElement) {
          resultElement.innerText = response.toString(); // Assuming your response has a "result" property
        }
      },
      (error) => {
        console.error(error); // Log the error
      }
    );
  }

  private performOperation(currentOperation: any): Promise<number> {
    switch (currentOperation.operator) {
      case '+':
        return this.performAddition(currentOperation.operand);
      case '-':
        return this.performSubtraction(currentOperation.operand);
      default:
        return Promise.reject(new Error('Unsupported operation'));
    }
  }

  private performAddition(numberToAdd: number): Promise<number> {
    const additionEndpoint = '/addition/';
    return new Promise((resolve, reject) => {
      this.http.post(additionEndpoint, {Number1: this.calculationCount(), Number2: numberToAdd}).subscribe(
        (response: any) => {
          this.calculationCount.set(response);
          resolve(response.result); // Assuming the backend returns the result under the key "result"
        },
        (error) => {
          console.error(error);
          reject(error);
        }
      );
    });
  }

  private performSubtraction(numberToAdd: number): Promise<number> {
    const subtractionEndpoint = '/subtraction/';
    return new Promise((resolve, reject) => {
      this.http.post(subtractionEndpoint, {Number1: this.calculationCount(), Number2: numberToAdd}).subscribe(
        (response: any) => {
          this.calculationCount.set(response);
          resolve(response.result);
        },
        (error) => {
          console.error(error);
          reject(error);
        }
      );
    });
  }

  async onResultClickTest() {
    const finalString = this.tempCount() + ' ' + this.count();
    let operations = this.parseOperations(finalString);

    if (this.calculationCount() === 0) {
      operations[0].operator = '+';
    } else {
      operations = operations.slice(1);
    }

    for (const operation of operations) {
      await this.performOperation(operation);
    }

    this.tempCount.set('');
    this.count.set(this.calculationCount().toString());
  }
}

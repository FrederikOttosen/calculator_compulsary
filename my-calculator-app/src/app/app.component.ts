import {Component, signal} from '@angular/core';
import {HttpClient} from "@angular/common/http";
import {HistoryEntity} from './Models/historyEntity';

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
  history = signal<HistoryEntity[]>([])

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
        let endpoint = '/addition/';
        return this.performCalculation(endpoint, currentOperation.operand);
      case '-':
        let endpointSub = '/subtraction/';
        return this.performCalculation(endpointSub, currentOperation.operand);
      case '/':
        let endpointDiv = '/division/';
        return this.performCalculation(endpointDiv, currentOperation.operand);
      case '*':
        let endpointMulti = '/multiplication/';
        return this.performCalculation(endpointMulti, currentOperation.operand);
      default:
        return Promise.reject(new Error('Unsupported operation'));
    }
  }

  public performCalculation(endpoint: string, numberToAdd: number): Promise<number> {
    return new Promise((resolve, reject) => {
      this.http.post(endpoint, {Number1: this.calculationCount(), Number2: numberToAdd}).subscribe(
        (response: any) => {
          console.log(response)
          console.log(response.history as HistoryEntity[])
          this.calculationCount.set(response.response);
          this.history.set(response.history)
          resolve(response.response);
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

    if (this.calculationCount() !== operations[0].operand) {
      this.calculationCount.set(0);
      operations[0].operator = '+';
    } else {
      operations = operations.slice(1);
    }

    for (const operation of operations) {
      if (!operation.operand) {
        operation.operand = 0;
      }
      await this.performOperation(operation);
    }

    this.tempCount.set('');
    this.count.set(this.calculationCount().toString());
  }
}

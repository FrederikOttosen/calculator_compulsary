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
  loading = false

  constructor(
    private http: HttpClient) {
  }

  onSignChangeClick() {
    let currentVal = this.count();
    if (currentVal.startsWith('-')) {
      this.count.set(currentVal.slice(1));
    } else {
      this.count.set('-' + currentVal);
    }
  }

  onClickClearCount() {
    this.count.set('');
  }

  onClickClearAll() {
    this.clearHistory();
    this.history.set([])
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
    const parts = finalString.split(' ').filter(Boolean);
    const operations: any = [];
    let currentOperator: string | null = null;

    parts.forEach((part) => {
      if (['+', '-', '*', '/'].includes(part)) {
        currentOperator = part;
      } else {
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
          const parsedHistory = response.history as HistoryEntity[]
          this.calculationCount.set(response.response);
          this.history.set(parsedHistory)
          resolve(response.response);
        },
        (error) => {
          reject(error);
        }
      );
    });
  }

  async onResultClickTest() {
    const finalString = this.tempCount() + ' ' + this.count();
    let operations = this.parseOperations(finalString);

    if (this.calculationCount() !== operations[0].operand) {
      this.calculationCount.set(operations[0].operand);
      operations = operations.slice(1);
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

    this.loading = true;
    setTimeout(() => {
      this.fetchHistory();
    }, 3000);
  }

  onClickStress() {
    const operators = ["+", "-", "*", "/"];
    const minNumbers = 51;
    const maxNumbers = 51;

    // Generate a random number of operands between minNumbers and maxNumbers
    const numOperands = Math.floor(Math.random() * (maxNumbers - minNumbers + 1)) + minNumbers;

    // Initialize the calculation string with the first random number
    let calculationString = Math.floor(Math.random() * 1000).toString();

    // Add random operators and numbers to the calculation string
    for (let i = 1; i < numOperands; i++) {
      const operator = operators[Math.floor(Math.random() * operators.length)];
      const operand = Math.floor(Math.random() * 1000);
      calculationString += ` ${operator} ${operand}`;
    }

    this.tempCount.set(calculationString)
  }

  private fetchHistory() {
      this.http.get('/storage-handler/').subscribe((response: any) => {
        console.log(response)
        const parsedHistory = response as HistoryEntity[]
        this.history.set(parsedHistory)
        this.loading = false
        }
      );
  }

  clearHistory(){
    console.log('called my endpoint')
    this.http.delete('/storage-handler/').subscribe(() => {
      console.log('cleared');
      }
    );
  }
}

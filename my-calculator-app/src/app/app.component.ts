import {Component, OnInit, signal} from '@angular/core';
import {HttpClient} from "@angular/common/http";
import {single} from "rxjs";

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

  count = signal<string>('');
  tempCount = signal<string>('');
  calculationCount = signal<number>(0);

  constructor(
    private http: HttpClient) {
  }

  onClickClearCount() {
    this.count.set('');
  }

  onClickClearAll() {
    this.count.set('');
    this.tempCount.set('');
  }


/*  onResultClick() {
    const finalString = this.tempCount() + this.count();
    const operations = this.parseOperations(finalString);  // Assume parseOperations does the parsing as previously discussed

    operations.reduce((previousPromise, currentOperation) => {
      return previousPromise.then(() => this.performOperation(currentOperation));
    }, Promise.resolve())
      .then(() => {
        console.log('All operations completed!');
      })
      .catch((error: any) => {
        console.error('An error occurred:', error);
      });
  }*/

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



  public onAdditionClick(requestBody: requestBody) {
    const additionEndpoint = '/addition/';
    this.http.post(additionEndpoint, requestBody).subscribe(
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

  public onSubtractionClickOld(requestBody: requestBody) {
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

  public onAdditionClickOld(requestBody: requestBody) {
    const additionEndpoint = '/addition/';
    this.http.post(additionEndpoint, requestBody).subscribe(
      (response) => {
        console.log(response)
      },
      (error) => {
        console.error(error); // Log the error
      });
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
        return this.performAddition(currentOperation);
      case '-':
        //return this.performSubtraction(currentOperation);
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

  async onResultClickTest() {
    const finalString = this.tempCount() + ' ' + this.count();
    const operations = this.parseOperations(finalString);

    operations[0].operator = '+';

    console.log(operations)

    for (const operation of operations) {
      await this.performOperation(operation);
    }
  }
}

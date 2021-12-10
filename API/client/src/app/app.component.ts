import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  title = 'The Dating App';
  users: any;
  private httpOptions: any;

  constructor(private http: HttpClient) {
    // this.httpOptions = {
    //   observe: 'body', 
    //   responseType: 'json',
    //   /*
    //   headers: new HttpHeaders({ 
    //     'Access-Control-Allow-Origin':'*'
    //   })
    //   */
    //   headers:{'Access-Control-Allow-Origin':'*'}
    // };
  }

  ngOnInit(): void {

    this.GetUsers();
  }
  GetUsers() {
    this.http.get('https://localhost:5001/api/users').subscribe(response => {
      this.users = response;
    }, error => {
      console.log(error);
    })
  }

}

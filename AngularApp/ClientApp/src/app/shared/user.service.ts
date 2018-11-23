

import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Response } from "@angular/http";
import { Observable } from 'rxjs';
import 'rxjs/add/operator/map';
import { User } from './user';


@Injectable()
export class UserService {

  readonly rootUrl = 'http://localhost:56874';

  constructor(private http: HttpClient) { }


  registerUser(user: User) {
    const body: User = {

      UserName: user.UserName,
      Email: user.Email,
      FirstName: user.FirstName,
      LastName: user.LastName,
      Password: user.Password
    }

    return this.http.post(this.rootUrl + '/api/Account', body);
  }

}

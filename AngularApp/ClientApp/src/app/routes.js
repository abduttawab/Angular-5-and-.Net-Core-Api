"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var home_component_1 = require("./home/home.component");
var user_component_1 = require("./user/user.component");
var sign_up_component_1 = require("./user/sign-up/sign-up.component");
var sign_in_component_1 = require("./user/sign-in/sign-in.component");
exports.appRoutes = [
    { path: 'home', component: home_component_1.HomeComponent },
    {
        path: 'signup', component: user_component_1.UserComponent,
        children: [{ path: '', component: sign_up_component_1.SignUpComponent }]
    },
    {
        path: 'login', component: user_component_1.UserComponent,
        children: [{ path: '', component: sign_in_component_1.SignInComponent }]
    },
    { path: '', redirectTo: '/login', pathMatch: 'full' }
];
//# sourceMappingURL=routes.js.map
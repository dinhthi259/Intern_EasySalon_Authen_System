import React from 'react';
import ReactDOM from 'react-dom/client';
import App from './App';
import reportWebVitals from './reportWebVitals';
import GlobalStyles from './components/GlobalStyles';
import { GoogleOAuthProvider } from '@react-oauth/google';
import "stream-chat-react/dist/css/index.css";

const root = ReactDOM.createRoot(document.getElementById('root'));
root.render(
  <React.StrictMode>
    <GoogleOAuthProvider clientId="386032350723-05tas9vtbgtfqqcfqluq6375h0i3kp8s.apps.googleusercontent.com">
      <GlobalStyles>
        <App />
      </GlobalStyles>
    </GoogleOAuthProvider>
  </React.StrictMode>
);

// If you want to start measuring performance in your app, pass a function
// to log results (for example: reportWebVitals(console.log))
// or send to an analytics endpoint. Learn more: https://bit.ly/CRA-vitals
reportWebVitals();

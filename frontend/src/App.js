import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import DefaultLayout from "./Layout/DefaultLayout";
import routes from "./routes";
import { AdminRoute, ProtectedRoute, PublicRoutes } from "./routes/Routes.js";
import { ToastContainer, Bounce } from "react-toastify";
import "react-toastify/dist/ReactToastify.css";
import FloatingChatBubble from "./components/FloatingChatBubble";

function App() {
  return (
    <Router>
      <div className="App">
        <Routes>
          {routes.map((route, index) => {
            const Page = route.component;

            // layout mặc định
            let Layout = DefaultLayout;

            if (route.layout === null) {
              Layout = ({ children }) => <>{children}</>;
            } else if (route.layout) {
              Layout = route.layout;
            }

            let element = <Page />;

            if (route.AdminRoute) {
              element = <AdminRoute>{element}</AdminRoute>;
            }

            // protected route
            if (route.protected) {
              element = <ProtectedRoute>{element}</ProtectedRoute>;
            }

            // public route
            if (route.publicOnly) {
              element = <PublicRoutes>{element}</PublicRoutes>;
            }

            element = <Layout>{element}</Layout>;

            return <Route key={index} path={route.path} element={element} />;
          })}
        </Routes>
        <ToastContainer
          position="top-right"
          autoClose={4000}
          theme="colored"
          transition={Bounce}
        />
        <FloatingChatBubble />
      </div>
    </Router>
  );
}

export default App;

// layouts
import DefaultLayout from "../Layout/DefaultLayout";
import SettingsLayout from "../Layout/SidebarLayout";
import AdminLayout from "../Layout/AdminLayout";
// pages
import Home from "../pages/Home/Block_Cate_Banner";
import Register from "../pages/Register";
import LogIn from "../pages/LogIn";
import VerifyEmail from "../pages/VerifyEmail";
import ResetPassword from "../pages/ResetPassword";
import InputEmailReset from "../pages/InputEmailReset";
import AccountSetting from "../pages/AccountSetting";
import CategoryPage from "../pages/CategoryPage";
import ProductDetail from "../pages/ProductDetail";
import ActiveSessionsPage from "../pages/ActiveSessionPage";
import CartPage from "../pages/CartPage";
import ProfilePage from "../pages/ProfilePage";
import AddressLisstPage from "../pages/AddressListPage";
import CheckoutPage from "../pages/CheckoutPage";
import AdminPage from "../pages/AdminPage";
import InventoryPage from "../pages/InventoryPage";
import AdminCategoryPage from "../pages/AdminCategoryPage";
import AdminProductPage from "../pages/AdminProductPage";
import OrderSuccess from "../pages/OrderSuccess";
import MyOrderPage from "../pages/MyOrderPage";
import AdminOrderPage from "../pages/AdminOrderPage";
import AdminUserPage from "../pages/AdminUserPage";
import SupplierPage from "../pages/SupplierPage";
import ReviewManagement from "../pages/Seller/ReviewManagement/ReviewManagement";
import SearchResults from "../pages/SearchResults";
import StatisticsDashboard from "../components/Dashboard/Statistics/StatisticsDashboard";
import PaymentCancelPage from "../pages/PaymentPage/PaymentCancelPage";
import PaymentSuccessPage from "../pages/PaymentPage/PaymentSuccessPage";
import BankAccountPage from "../pages/BankAccountPage";
import AdminInvoicePage from "../pages/AdminInvoicePage";
import AdminTaxPage from "../pages/AdminTaxPage";
import AdminInventoryInvoicesPage from "../pages/AdminInventoryInvoicePage";

import { ProtectedRoute } from "./Routes";
import { AdminRoute } from "./Routes";
const routes = [
  {
    path: "/",
    component: Home,
    layout: DefaultLayout
  },
  {
    path: "/register",
    component: Register,
    layout: DefaultLayout,
    publicOnly: true
  },
  {
    path: "/login",
    component: LogIn,
    layout: DefaultLayout,
    publicOnly: true
  },
  {
    path: "/email-verify",
    component: VerifyEmail,
    layout: DefaultLayout,
  },
  {
    path: "/reset-password",
    component: ResetPassword,
    layout: DefaultLayout,
    publicOnly: true
  },
  {
    path: "/input-email-reset",
    component: InputEmailReset,
    layout: DefaultLayout,
    publicOnly: true
  },
  {
    path: "/account-setting",
    component: AccountSetting,
    layout: DefaultLayout,
    ProtectedRoute: true
  },
  {
    path: "/category/:slug",
    component: CategoryPage,
    layout: DefaultLayout
  },
  {
    path: "/product/:slug",
    component: ProductDetail,
    layout: DefaultLayout
  },
  {
    path: "/session",
    component: ActiveSessionsPage,
    layout: SettingsLayout,
    ProtectedRoute: true
  },
  {
    path: "/cart",
    component: CartPage,
    layout: DefaultLayout,
    ProtectedRoute: true
  },
  {
    path: "/profile",
    component: ProfilePage,
    layout: SettingsLayout,
    ProtectedRoute: true
  },
  {
    path: "/address",
    component: AddressLisstPage,
    layout: SettingsLayout,
    ProtectedRoute: true
  },
  {
    path: "/search",
    component: SearchResults,
    layout: DefaultLayout,
  },
  {
    path: "/checkout",
    component: CheckoutPage,
    layout: DefaultLayout,
    ProtectedRoute: true
  },
  {
    path: "/order-success",
    component: OrderSuccess,
    layout: DefaultLayout,
    ProtectedRoute: true
  },
  {
    path: "/my-orders",
    component: MyOrderPage,
    layout: SettingsLayout,
    ProtectedRoute: true
  },
  {
    path: "/payment-success",
    component: PaymentSuccessPage,
    layout: DefaultLayout,
    ProtectedRoute: true
  },
  {
    path: "/payment-cancel",
    component: PaymentCancelPage,
    layout: DefaultLayout,
    ProtectedRoute: true
  },
  {
    path: "/my-orders",
    component: MyOrderPage,
    layout: SettingsLayout,
    ProtectedRoute: true
  },
  {
    path: "/bank-accounts",
    component: BankAccountPage,
    layout: SettingsLayout,
    ProtectedRoute: true
  },
  {
    path: "/admin",
    component: AdminPage,
    layout: AdminLayout,
    ProtectedRoute: true,
    AdminRoute: true
  },
  {
    path: "/admin/inventory",
    component: InventoryPage,
    layout: AdminLayout,
    ProtectedRoute: true,
    AdminRoute: true
  },
  {
    path: "/admin/category",
    component: AdminCategoryPage,
    layout: AdminLayout,
    ProtectedRoute: true,
    AdminRoute: true
  },
  {
    path: "/admin/products",
    component: AdminProductPage,
    layout: AdminLayout,
    ProtectedRoute: true,
    AdminRoute: true
  },
  {
    path: "/admin/orders",
    component: AdminOrderPage,
    layout: AdminLayout,
    ProtectedRoute: true,
    AdminRoute: true
  },
  {
    path: "/admin/customers",
    component: AdminUserPage,
    layout: AdminLayout,
    ProtectedRoute: true,
    AdminRoute: true
  },
  {
    path: "/admin/suppliers",
    component: SupplierPage,
    layout: AdminLayout,
    ProtectedRoute: true,
    AdminRoute: true
  },
  {
    path: "/admin/review",
    component: ReviewManagement,
    layout: AdminLayout,
    ProtectedRoute: true,
    AdminRoute: true
  },
  {
    path: "/admin/dashboard",
    component: StatisticsDashboard,
    layout: AdminLayout,
    ProtectedRoute: true,
    AdminRoute: true
  },
  {
    path: "/admin/documents/invoices",
    component: AdminInvoicePage,
    layout: AdminLayout,
    ProtectedRoute: true,
    AdminRoute: true
  },
  {
    path: "/admin/documents/tax-declarations",
    component: AdminTaxPage,
    layout: AdminLayout,
    ProtectedRoute: true,
    AdminRoute: true
  },
  {
    path: "/admin/documents/inventory-invoices",
    component: AdminInventoryInvoicesPage,
    layout: AdminLayout,
    ProtectedRoute: true,
    AdminRoute: true
  }


];

export default routes;
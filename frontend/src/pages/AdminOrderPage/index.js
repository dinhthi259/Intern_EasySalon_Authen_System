import { useEffect, useState } from "react";
import classNames from "classnames/bind";
import styles from "./AdminOrderPage.module.scss";
import {
  editOrderStatus,
  cancelOrder,
  getAdminOrders,
  getAdminCountByStatus,
} from "../../api/OrderApi";
import { confirmRefund, getRefundDetails } from "../../api/RefundApi";

import ConfirmDialog from "../../components/ConfirmDialog";
import { notifyError, notifySuccess } from "../../components/Nofitication";

import OrderTab from "./components/OrderTab";
// import OrderSearch from "../MyOrderPage/components/OrderSearch";
import OrderList from "./components/OrderList";
import EmptyState from "../MyOrderPage/components/EmptyState";
import OrderSearch from "./components/OrderSearch";
import AdminRefundModal from "./components/AdminRefundModal";

const cx = classNames.bind(styles);

export default function MyOrderPage() {
  const [status, setStatus] = useState("");
  const [keyword, setKeyword] = useState({});
  const [orders, setOrders] = useState([]);
  const [showConfirm, setShowConfirm] = useState(false);
  const [deleteId, setDeleteId] = useState(null);
  const [countByStatus, setCountByStatus] = useState({});
  const [selectedRefundOrder, setSelectedRefundOrder] = useState(null);
  const [showRefundModal, setShowRefundModal] = useState(false);
  const [refundLoading, setRefundLoading] = useState(false);
  const [refundRequests, setRefundRequests] = useState([]);

  const handleOpenRefund = (order) => {
    const refund = refundRequests.find(
      (x) => x.orderId === order.id && x.status === "Pending",
    );

    if (!refund) {
      notifyError("Không tìm thấy yêu cầu hoàn tiền của đơn hàng này");
      return;
    }

    setSelectedRefundOrder({
      id: refund.id,
      amount: refund.amount,
      bankName: refund.bankName,
      bankLogo: refund.bankLogo,
      bankAccountNumber: refund.bankAccountNumber,
      bankAccountName: refund.bankAccountName,
      reason: refund.reason,
    });

    setShowRefundModal(true);
  };

  const fetchOrders = async () => {
    const res = await getAdminOrders({ status, keyword });
    setOrders(res.data);
  };

  const fetchCounts = async () => {
    const res = await getAdminCountByStatus();

    const map = {};
    res.data.forEach((item) => {
      map[item.status] = item.count;
    });

    setCountByStatus(map);
  };

  const fetchRefundRequests = async () => {
    const res = await getRefundDetails();
    setRefundRequests(res.data);
  };

  const handleCancelOrder = async (orderId) => {
    try {
      const res = await cancelOrder(orderId);
      fetchOrders();
      notifySuccess("Hủy đơn hàng thành công");
    } catch (error) {
      const message = error.response?.data?.message || "Hủy đơn hàng thất bại";
      notifyError(message);
    } finally {
      setShowConfirm(false);
    }
  };

  const handleEditStatus = async (orderId, status) => {
    try {
      await editOrderStatus(orderId, status);
      fetchOrders();
      notifySuccess("Cập nhật trạng thái đơn hàng thành công");
    } catch (error) {
      const message =
        error.response?.data?.message ||
        "Cập nhật trạng thái đơn hàng thất bại";
      notifyError(message);
    }
  };

  const handleSearch = async (form) => {
    const res = await getAdminOrders({
      status,
      ...form, // 🔥 spread object
    });
    setOrders(res.data);
    if (res.data.length === 0) {
      notifyError("Không tìm thấy đơn hàng nào phù hợp");
    } else {
      notifySuccess(`Tìm thấy ${res.data.length} đơn hàng phù hợp`);
    }
  };

  const handleAskCancel = (orderId) => {
    setDeleteId(orderId);
    setShowConfirm(true);
  };

  const handleConfirmRefund = async (refundId) => {
    try {
      setRefundLoading(true);

      await confirmRefund(refundId);

      notifySuccess("Đã xác nhận hoàn tiền thành công");

      setShowRefundModal(false);
      setSelectedRefundOrder(null);

      fetchOrders();
      fetchCounts();
    } catch (error) {
      const message =
        error.response?.data?.message ||
        error.response?.data ||
        "Xác nhận hoàn tiền thất bại";

      notifyError(message);
    } finally {
      setRefundLoading(false);
    }
  };

  useEffect(() => {
    fetchOrders();
    fetchCounts();
    fetchRefundRequests();
  }, [status, keyword]);

  return (
    <div className={cx("wrapper")}>
      <div className={cx("topBar")}>
        <h2 className={cx("title")}>Quản lý đơn hàng</h2>
        {/* <OrderSearch onSearch={setKeyword} /> */}
      </div>

      <div className={cx("content")}>
        <OrderTab onChange={setStatus} counts={countByStatus} />
        <OrderSearch onSearch={handleSearch} />

        <div className={cx("list")}>
          {orders.length === 0 ? (
            <EmptyState />
          ) : (
            <OrderList
              orders={orders}
              onCancel={handleAskCancel}
              onEditStatus={handleEditStatus}
              onOpenRefund={handleOpenRefund}
              refresh={fetchOrders}
            />
          )}
        </div>
      </div>
      <AdminRefundModal
        open={showRefundModal}
        onClose={() => setShowRefundModal(false)}
        refund={selectedRefundOrder}
        loading={refundLoading}
        onConfirm={handleConfirmRefund}
      />
      <ConfirmDialog
        open={showConfirm}
        title="Hủy đơn hàng"
        message="Bạn có chắc muốn hủy đơn hàng này?"
        confirmText="Xác nhận"
        cancelText="Hủy"
        onConfirm={() => handleCancelOrder(deleteId)}
        onCancel={() => setShowConfirm(false)}
      />
    </div>
  );
}

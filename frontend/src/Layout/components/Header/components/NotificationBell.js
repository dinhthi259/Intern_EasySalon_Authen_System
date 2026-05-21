import { useEffect, useState } from "react";
import * as signalR from "@microsoft/signalr";
import {
  fetchNotifications,
  markNotificationAsRead,
} from "../../../../api/NotificationApi";
import styles from "./NotificationBell.module.scss";
import classNames from "classnames/bind";
import { FaBell } from "react-icons/fa";

const cx = classNames.bind(styles);

function NotificationBell({ userId }) {
  const [notifications, setNotifications] = useState([]);
  const [open, setOpen] = useState(false);
  const API_URL = process.env.REACT_APP_API_URL;

  useEffect(() => {
    if (!userId) {
      setNotifications([]);
      setOpen(false);
      return;
    }

    const loadNotifications = async () => {
      try {
        const res = await fetchNotifications(userId);
        setNotifications(res.data);
      } catch (err) {
        console.error("Lỗi lấy notification:", err);
      }
    };

    loadNotifications();
  }, [userId]);

  useEffect(() => {
    if (!userId) return;

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(`${API_URL}/notificationHub`)
      .withAutomaticReconnect()
      .build();

    connection.on("ReceiveNotification", (notification) => {
      console.log("Realtime notification nhận được:", notification);

      setNotifications((prev) => [notification, ...prev]);
    });

    connection.on("NotificationRead", (notificationId) => {
      setNotifications((prev) =>
        prev.map((item) =>
          item.id === notificationId ? { ...item, isRead: true } : item,
        ),
      );
    });

    connection
      .start()
      .then(() => {
        console.log("SignalR connected");
        console.log("userId đang join:", userId);
        connection.invoke("JoinUserGroup", userId.toString());
      })
      .then(() => {
        console.log("Join group thành công:", `user_${userId}`);
      })
      .catch((err) => console.error("SignalR error:", err));

    return () => {
      connection.off("ReceiveNotification");
      connection.off("NotificationRead");

      connection.stop().catch((err) => {
        console.error("SignalR stop error:", err);
      });
    };
  }, [userId]);

  const unreadCount = notifications.filter((n) => !n.isRead).length;

  const markAsRead = async (notification) => {
    try {
      if (!notification.isRead) {
        await markNotificationAsRead(notification.id);

        setNotifications((prev) =>
          prev.map((item) =>
            item.id === notification.id ? { ...item, isRead: true } : item,
          ),
        );
      }

      if (notification.link) {
        window.location.href = notification.link;
      }
    } catch (error) {
      console.error("Lỗi đánh dấu đã đọc:", error);
    }
  };

  return (
    <div className={cx("wrapper")}>
      <button className={cx("bellBtn")} onClick={() => setOpen(!open)}>
        <FaBell />
        {unreadCount > 0 && <span className={cx("badge")}>{unreadCount}</span>}
      </button>

      {open && (
        <div className={cx("dropdown")}>
          <div className={cx("header")}>Thông báo</div>

          {notifications.length === 0 ? (
            <div className={cx("empty")}>Không có thông báo</div>
          ) : (
            notifications.map((item) => (
              <div
                key={item.id}
                className={cx("notificationItem", { unread: !item.isRead })}
                onClick={() => markAsRead(item)}
              >
                <div className={cx("title")}>{item.title}</div>
                <div className={cx("message")}>{item.message}</div>
                <div className={cx("time")}>
                  {new Date(item.createdAt).toLocaleString()}
                </div>
              </div>
            ))
          )}
        </div>
      )}
    </div>
  );
}

export default NotificationBell;

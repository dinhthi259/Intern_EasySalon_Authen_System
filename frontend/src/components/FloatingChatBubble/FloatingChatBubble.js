import { useState, useEffect } from "react";
import classNames from "classnames/bind";
import styles from "./FloatingChatBubble.module.scss";
import { FaRobot, FaTimes, FaHeadset, FaComments } from "react-icons/fa";
import FloatingChatModal from "./FloatingChatModal";
import SellerChatBox from "./SellerChatBox";

const cx = classNames.bind(styles);

export default function FloatingChatBubble() {
  const [menuOpen, setMenuOpen] = useState(false);
  const [chatMode, setChatMode] = useState(null);
  const [isAuthenticated, setIsAuthenticated] = useState(false);

  useEffect(() => {
    const token = localStorage.getItem("accessToken");
    setIsAuthenticated(!!token);

    const handleAuthChange = () => {
      const newToken = localStorage.getItem("accessToken");
      setIsAuthenticated(!!newToken);
    };

    window.addEventListener("auth-change", handleAuthChange);
    return () => window.removeEventListener("auth-change", handleAuthChange);
  }, []);

  if (!isAuthenticated) return null;

  const openAiChat = () => {
    setMenuOpen(false);
    setChatMode("ai");
  };

  const openSellerChat = () => {
    setMenuOpen(false);
    setChatMode("seller");
  };

  const closeChat = () => {
    setChatMode(null);
  };

  return (
    <>
      {!chatMode && (
        <div
          className={cx("floatingBubble")}
          onClick={() => setMenuOpen((prev) => !prev)}
        >
          {menuOpen ? (
            <FaTimes className={cx("icon")} />
          ) : (
            <FaComments className={cx("icon")} />
          )}
          <span className={cx("label")}>Chat</span>
        </div>
      )}

      {menuOpen && !chatMode && (
        <div className={cx("chatMenu")}>
          <button onClick={openAiChat}>
            <FaRobot />
            <div>
              <strong>Trợ lý AI</strong>
              <span>Tư vấn sản phẩm tự động</span>
            </div>
          </button>

          <button onClick={openSellerChat}>
            <FaHeadset />
            <div>
              <strong>Chat với nhân viên</strong>
              <span>Trao đổi trực tiếp với shop</span>
            </div>
          </button>
        </div>
      )}

      {chatMode === "ai" && (
        <FloatingChatModal
          isOpen
          onClose={closeChat}
          onOpenSellerChat={() => setChatMode("seller")}
        />
      )}

      {chatMode === "seller" && (
        <div className={cx("sellerChatBox")}>
          <div className={cx("sellerChatHeader")}>
            <span>Chat với nhân viên</span>
            <button onClick={closeChat}>×</button>
          </div>

          <SellerChatBox />
        </div>
      )}
    </>
  );
}
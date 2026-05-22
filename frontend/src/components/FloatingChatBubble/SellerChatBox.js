import { useEffect, useState } from "react";
import { StreamChat } from "stream-chat";
import { getUserFromToken } from "../../helper/JwtDecodeHelper";
import {
  Chat,
  Channel,
  Window,
  ChannelHeader,
  MessageList,
  MessageComposer,
} from "stream-chat-react";
import { getStreamChatToken } from "../../api/StreamChatApi";
import styles from "./SellerChatBox.module.scss"
import classNames from "classnames/bind";

const cx = classNames.bind(styles)

function SellerChatBox() {
  const [client, setClient] = useState(null);
  const [channel, setChannel] = useState(null);

  useEffect(() => {
    let chatClient;
    let isMounted = true;
    const user = getUserFromToken();
    const currentUserId = user?.userId;

    const init = async () => {
      try {
        const res = await getStreamChatToken(currentUserId);

        const userId = String(res.data.userId);
        const sellerId = String(res.data.sellerId || "admin_1");
        const channelId = `customer_${userId}`;

        chatClient = StreamChat.getInstance(res.data.apiKey);

        await chatClient.connectUser(
          {
            id: userId,
            name: res.data.email || "Khách hàng",
          },
          res.data.token,
        );

        const chatChannel = chatClient.channel("messaging", channelId, {
          members: [userId, sellerId],
        });

        await chatChannel.watch();

        if (isMounted) {
          setClient(chatClient);
          setChannel(chatChannel);
        }
      } catch (error) {
        console.error("Connect seller chat failed:", error);
      }
    };

    init();

    return () => {
      isMounted = false;

      if (chatClient) {
        chatClient.disconnectUser();
      }
    };
  }, []);

  if (!client || !channel) {
    return <p>Đang kết nối người bán...</p>;
  }

  return (
    <div className={cx("seller-chat-wrapper")}>
      <Chat client={client}>
        <Channel channel={channel}>
          <Window>
            <ChannelHeader />

            <div className={cx("message-list-wrapper")}>
              <MessageList />
            </div>

            <div className={cx("message-input-wrapper")}>
              <MessageComposer />
            </div>
          </Window>
        </Channel>
      </Chat>
    </div>
  );
}

export default SellerChatBox;

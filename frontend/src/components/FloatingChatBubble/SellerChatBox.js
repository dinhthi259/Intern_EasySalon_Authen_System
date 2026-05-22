import { useEffect, useState } from "react";
import { StreamChat } from "stream-chat";
import {
  Chat,
  Channel,
  Window,
  ChannelHeader,
  MessageList,
  MessageComposer,
} from "stream-chat-react";
import { getStreamChatToken } from "../../api/StreamChatApi";

function SellerChatBox() {
  const [client, setClient] = useState(null);
  const [channel, setChannel] = useState(null);

  useEffect(() => {
    let chatClient;
    let isMounted = true;

    const init = async () => {
      try {
        const res = await getStreamChatToken();

        const userId = String(res.data.userId);
        const sellerId = "admin_1";

        chatClient = StreamChat.getInstance(res.data.apiKey);

        await chatClient.connectUser(
          {
            id: userId,
            name: res.data.fullName || "Khách hàng",
          },
          res.data.token
        );

        const chatChannel = chatClient.channel(
          "messaging",
          `buyer_${userId}_seller_${sellerId}`,
          {
            members: [userId, sellerId],
          }
        );

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
    <Chat client={client}>
      <Channel channel={channel}>
        <Window>
          <ChannelHeader />
          <MessageList />
          <MessageComposer />
        </Window>
      </Channel>
    </Chat>
  );
}

export default SellerChatBox;
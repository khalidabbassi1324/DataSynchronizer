import asyncio
import socket

HOST = '127.0.0.1'
PORT = 5555
clients = []

# This message is an example; it will be dynamically updated by the server.
message = ""  # Initialize a message to be broadcast

async def handle_client(reader, writer):
    address = writer.get_extra_info('peername')
    clients.append(writer)
    print(f"New client connected: {address}")

    try:
        while True:
            data = await reader.read(100)
            if not data:
                break
            message = data.decode()
            print(f"Received: {message}")
            await broadcast_message(message)
    except asyncio.CancelledError:
        pass
    except Exception as e:
        print(f"Error: {e}")
    finally:
        disconnect_client(writer)

async def broadcast_message(message):
    # We are making a copy of the list to avoid modifying the list while iterating
    for client in list(clients):
        try:
            client.write(message.encode())
            await client.drain()
        except:
            clients.remove(client)

def disconnect_client(writer):
    if writer in clients:
        clients.remove(writer)
    writer.close()
    print(f"Client disconnected: {writer.get_extra_info('peername')}")

async def main():
    server = await asyncio.start_server(
        handle_client, HOST, PORT
    )
    addr = server.sockets[0].getsockname()
    print(f"Server listening on {addr}")

    # Serve requests forever
    async with server:
        await server.serve_forever()

if __name__ == '__main__':
    asyncio.run(main())

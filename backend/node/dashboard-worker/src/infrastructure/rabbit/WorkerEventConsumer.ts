import {
  connect,
  type Channel,
  type ChannelModel,
  type ConsumeMessage
} from 'amqplib';
import { logger } from '../logger.js';

type EventEnvelope = {
  eventType: string;
  occurredAt?: string;
  payload?: unknown;
};

type ConsumerOptions = {
  url: string;
  exchangePrefix: string;
  queueName: string;
  prefetch: number;
  eventTypes: string[];
};

type ConsumerHandle = {
  close: () => Promise<void>;
};

function buildExchangeName(exchangePrefix: string, eventType: string): string {
  if (!exchangePrefix || exchangePrefix.trim().length === 0) {
    return eventType;
  }

  return `${exchangePrefix}.${eventType}`;
}

function parseEnvelope(message: ConsumeMessage): EventEnvelope {
  const raw = message.content.toString('utf-8');
  const parsed = JSON.parse(raw) as Record<string, unknown>;
  const eventType =
    (parsed.EventType as string | undefined) ??
    (parsed.eventType as string | undefined) ??
    'Unknown';

  return {
    eventType,
    occurredAt:
      (parsed.OccurredAt as string | undefined) ??
      (parsed.occurredAt as string | undefined),
    payload:
      (parsed.Payload as unknown | undefined) ??
      (parsed.payload as unknown | undefined)
  };
}

export async function startEventConsumer(
  options: ConsumerOptions,
  onEvent: (event: EventEnvelope) => Promise<void>
): Promise<ConsumerHandle> {
  const connection = await connect(options.url, {
    clientProperties: {
      connection_name: 'dashboard-worker'
    }
  });
  const channel = await connection.createChannel();
  await channel.prefetch(options.prefetch);

  await channel.assertQueue(options.queueName, {
    durable: true
  });

  for (const eventType of options.eventTypes) {
    const exchangeName = buildExchangeName(options.exchangePrefix, eventType);
    await channel.assertExchange(exchangeName, 'direct', { durable: true });
    await channel.bindQueue(options.queueName, exchangeName, eventType);
  }

  await channel.consume(
    options.queueName,
    async (message: ConsumeMessage | null) => {
      if (!message) {
        return;
      }

      try {
        const envelope = parseEnvelope(message);
        await onEvent(envelope);
        channel.ack(message);
      } catch (error) {
        logger.error('Failed to process dashboard event.', error);
        channel.nack(message, false, true);
      }
    },
    { noAck: false }
  );

  logger.info('Dashboard event consumer started.', {
    queueName: options.queueName,
    eventTypes: options.eventTypes
  });

  return buildConsumerHandle(connection, channel);
}

function buildConsumerHandle(
  connection: ChannelModel,
  channel: Channel
): ConsumerHandle {
  return {
    close: async () => {
      await channel.close();
      await connection.close();
    }
  };
}

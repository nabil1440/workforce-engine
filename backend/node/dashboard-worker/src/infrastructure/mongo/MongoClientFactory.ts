import { MongoClient } from 'mongodb';

export function createMongoClient(connectionString: string): MongoClient {
  return new MongoClient(connectionString);
}

import { ApolloClient, InMemoryCache, HttpLink, split } from '@apollo/client/core';
import { WebSocketLink } from '@apollo/client/link/ws';
import { getMainDefinition } from '@apollo/client/utilities';
import { APOLLO_OPTIONS } from 'apollo-angular';

export function createApollo() {
  const http = new HttpLink({ uri: 'http://localhost:5000/graphql' });
  const ws = new WebSocketLink({
    uri: 'ws://localhost:5000/graphql',
    options: { reconnect: true },
  });

  const link = split(
    ({ query }) => {
      const def = getMainDefinition(query);
      return def.kind === 'OperationDefinition' && def.operation === 'subscription';
    },
    ws,
    http
  );

  return {
    link,
    cache: new InMemoryCache(),
  };
}

export const APOLLO_PROVIDER = {
  provide: APOLLO_OPTIONS,
  useFactory: createApollo,
};

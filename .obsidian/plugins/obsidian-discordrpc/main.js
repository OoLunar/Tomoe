'use strict';

var require$$0 = require('electron');
var EventEmitter = require('events');
var require$$0$1 = require('timers');
var net = require('net');
var obsidian = require('obsidian');

function _interopDefaultLegacy (e) { return e && typeof e === 'object' && 'default' in e ? e : { 'default': e }; }

var require$$0__default = /*#__PURE__*/_interopDefaultLegacy(require$$0);
var EventEmitter__default = /*#__PURE__*/_interopDefaultLegacy(EventEmitter);
var require$$0__default$1 = /*#__PURE__*/_interopDefaultLegacy(require$$0$1);
var net__default = /*#__PURE__*/_interopDefaultLegacy(net);

/*! *****************************************************************************
Copyright (c) Microsoft Corporation.

Permission to use, copy, modify, and/or distribute this software for any
purpose with or without fee is hereby granted.

THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES WITH
REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF MERCHANTABILITY
AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY SPECIAL, DIRECT,
INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM
LOSS OF USE, DATA OR PROFITS, WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE OR
OTHER TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION WITH THE USE OR
PERFORMANCE OF THIS SOFTWARE.
***************************************************************************** */
/* global Reflect, Promise */

var extendStatics = function(d, b) {
    extendStatics = Object.setPrototypeOf ||
        ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
        function (d, b) { for (var p in b) if (Object.prototype.hasOwnProperty.call(b, p)) d[p] = b[p]; };
    return extendStatics(d, b);
};

function __extends(d, b) {
    extendStatics(d, b);
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
}

function __awaiter(thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
}

function __generator(thisArg, body) {
    var _ = { label: 0, sent: function() { if (t[0] & 1) throw t[1]; return t[1]; }, trys: [], ops: [] }, f, y, t, g;
    return g = { next: verb(0), "throw": verb(1), "return": verb(2) }, typeof Symbol === "function" && (g[Symbol.iterator] = function() { return this; }), g;
    function verb(n) { return function (v) { return step([n, v]); }; }
    function step(op) {
        if (f) throw new TypeError("Generator is already executing.");
        while (_) try {
            if (f = 1, y && (t = op[0] & 2 ? y["return"] : op[0] ? y["throw"] || ((t = y["return"]) && t.call(y), 0) : y.next) && !(t = t.call(y, op[1])).done) return t;
            if (y = 0, t) op = [op[0] & 2, t.value];
            switch (op[0]) {
                case 0: case 1: t = op; break;
                case 4: _.label++; return { value: op[1], done: false };
                case 5: _.label++; y = op[1]; op = [0]; continue;
                case 7: op = _.ops.pop(); _.trys.pop(); continue;
                default:
                    if (!(t = _.trys, t = t.length > 0 && t[t.length - 1]) && (op[0] === 6 || op[0] === 2)) { _ = 0; continue; }
                    if (op[0] === 3 && (!t || (op[1] > t[0] && op[1] < t[3]))) { _.label = op[1]; break; }
                    if (op[0] === 6 && _.label < t[1]) { _.label = t[1]; t = op; break; }
                    if (t && _.label < t[2]) { _.label = t[2]; _.ops.push(op); break; }
                    if (t[2]) _.ops.pop();
                    _.trys.pop(); continue;
            }
            op = body.call(thisArg, _);
        } catch (e) { op = [6, e]; y = 0; } finally { f = t = 0; }
        if (op[0] & 5) throw op[1]; return { value: op[0] ? op[1] : void 0, done: true };
    }
}

var _nodeResolve_empty = {};

var _nodeResolve_empty$1 = /*#__PURE__*/Object.freeze({
    __proto__: null,
    'default': _nodeResolve_empty
});

function createCommonjsModule(fn, basedir, module) {
	return module = {
		path: basedir,
		exports: {},
		require: function (path, base) {
			return commonjsRequire(path, (base === undefined || base === null) ? module.path : base);
		}
	}, fn(module, module.exports), module.exports;
}

function getAugmentedNamespace(n) {
	if (n.__esModule) return n;
	var a = Object.defineProperty({}, '__esModule', {value: true});
	Object.keys(n).forEach(function (k) {
		var d = Object.getOwnPropertyDescriptor(n, k);
		Object.defineProperty(a, k, d.get ? d : {
			enumerable: true,
			get: function () {
				return n[k];
			}
		});
	});
	return a;
}

function commonjsRequire () {
	throw new Error('Dynamic requires are not currently supported by @rollup/plugin-commonjs');
}

var require$$1 = /*@__PURE__*/getAugmentedNamespace(_nodeResolve_empty$1);

let register;
try {
  const { app } = require$$0__default['default'];
  register = app.setAsDefaultProtocolClient.bind(app);
} catch (err) {
  try {
    register = require$$1;
  } catch (e) {} // eslint-disable-line no-empty
}

if (typeof register !== 'function') {
  register = () => false;
}

function pid() {
  if (typeof process !== 'undefined') {
    return process.pid;
  }
  return null;
}

const uuid4122 = () => {
  let uuid = '';
  for (let i = 0; i < 32; i += 1) {
    if (i === 8 || i === 12 || i === 16 || i === 20) {
      uuid += '-';
    }
    let n;
    if (i === 12) {
      n = 4;
    } else {
      const random = Math.random() * 16 | 0;
      if (i === 16) {
        n = (random & 3) | 0;
      } else {
        n = random;
      }
    }
    uuid += n.toString(16);
  }
  return uuid;
};

var util = {
  pid,
  register,
  uuid: uuid4122,
};

var browser = createCommonjsModule(function (module, exports) {

// ref: https://github.com/tc39/proposal-global
var getGlobal = function () {
	// the only reliable means to get the global object is
	// `Function('return this')()`
	// However, this causes CSP violations in Chrome apps.
	if (typeof self !== 'undefined') { return self; }
	if (typeof window !== 'undefined') { return window; }
	if (typeof global !== 'undefined') { return global; }
	throw new Error('unable to locate global object');
};

var global = getGlobal();

module.exports = exports = global.fetch;

// Needed for TypeScript and Webpack.
if (global.fetch) {
	exports.default = global.fetch.bind(global);
}

exports.Headers = global.Headers;
exports.Request = global.Request;
exports.Response = global.Response;
});

const { uuid } = util;

const OPCodes = {
  HANDSHAKE: 0,
  FRAME: 1,
  CLOSE: 2,
  PING: 3,
  PONG: 4,
};

function getIPCPath(id) {
  if (process.platform === 'win32') {
    return `\\\\?\\pipe\\discord-ipc-${id}`;
  }
  const { env: { XDG_RUNTIME_DIR, TMPDIR, TMP, TEMP } } = process;
  const prefix = XDG_RUNTIME_DIR || TMPDIR || TMP || TEMP || '/tmp';
  return `${prefix.replace(/\/$/, '')}/discord-ipc-${id}`;
}

function getIPC(id = 0) {
  return new Promise((resolve, reject) => {
    const path = getIPCPath(id);
    const onerror = () => {
      if (id < 10) {
        resolve(getIPC(id + 1));
      } else {
        reject(new Error('Could not connect'));
      }
    };
    const sock = net__default['default'].createConnection(path, () => {
      sock.removeListener('error', onerror);
      resolve(sock);
    });
    sock.once('error', onerror);
  });
}

async function findEndpoint(tries = 0) {
  if (tries > 30) {
    throw new Error('Could not find endpoint');
  }
  const endpoint = `http://127.0.0.1:${6463 + (tries % 10)}`;
  try {
    const r = await browser(endpoint);
    if (r.status === 404) {
      return endpoint;
    }
    return findEndpoint(tries + 1);
  } catch (e) {
    return findEndpoint(tries + 1);
  }
}

function encode(op, data) {
  data = JSON.stringify(data);
  const len = Buffer.byteLength(data);
  const packet = Buffer.alloc(8 + len);
  packet.writeInt32LE(op, 0);
  packet.writeInt32LE(len, 4);
  packet.write(data, 8, len);
  return packet;
}

const working = {
  full: '',
  op: undefined,
};

function decode(socket, callback) {
  const packet = socket.read();
  if (!packet) {
    return;
  }

  let { op } = working;
  let raw;
  if (working.full === '') {
    op = working.op = packet.readInt32LE(0);
    const len = packet.readInt32LE(4);
    raw = packet.slice(8, len + 8);
  } else {
    raw = packet.toString();
  }

  try {
    const data = JSON.parse(working.full + raw);
    callback({ op, data }); // eslint-disable-line callback-return
    working.full = '';
    working.op = undefined;
  } catch (err) {
    working.full += raw;
  }

  decode(socket, callback);
}

class IPCTransport extends EventEmitter__default['default'] {
  constructor(client) {
    super();
    this.client = client;
    this.socket = null;
  }

  async connect() {
    const socket = this.socket = await getIPC();
    socket.on('close', this.onClose.bind(this));
    socket.on('error', this.onClose.bind(this));
    this.emit('open');
    socket.write(encode(OPCodes.HANDSHAKE, {
      v: 1,
      client_id: this.client.clientId,
    }));
    socket.pause();
    socket.on('readable', () => {
      decode(socket, ({ op, data }) => {
        switch (op) {
          case OPCodes.PING:
            this.send(data, OPCodes.PONG);
            break;
          case OPCodes.FRAME:
            if (!data) {
              return;
            }
            if (data.cmd === 'AUTHORIZE' && data.evt !== 'ERROR') {
              findEndpoint().then((endpoint) => {
                this.client.request.endpoint = endpoint;
              });
            }
            this.emit('message', data);
            break;
          case OPCodes.CLOSE:
            this.emit('close', data);
            break;
          default:
            break;
        }
      });
    });
  }

  onClose(e) {
    this.emit('close', e);
  }

  send(data, op = OPCodes.FRAME) {
    this.socket.write(encode(op, data));
  }

  close() {
    this.send({}, OPCodes.CLOSE);
    this.socket.end();
  }

  ping() {
    this.send(uuid(), OPCodes.PING);
  }
}

var ipc = IPCTransport;
var encode_1 = encode;
var decode_1 = decode;
ipc.encode = encode_1;
ipc.decode = decode_1;

function keyMirror(arr) {
  const tmp = {};
  for (const value of arr) {
    tmp[value] = value;
  }
  return tmp;
}


var browser$1 = typeof window !== 'undefined';

var RPCCommands = keyMirror([
  'DISPATCH',
  'AUTHORIZE',
  'AUTHENTICATE',
  'GET_GUILD',
  'GET_GUILDS',
  'GET_CHANNEL',
  'GET_CHANNELS',
  'CREATE_CHANNEL_INVITE',
  'GET_RELATIONSHIPS',
  'GET_USER',
  'SUBSCRIBE',
  'UNSUBSCRIBE',
  'SET_USER_VOICE_SETTINGS',
  'SET_USER_VOICE_SETTINGS_2',
  'SELECT_VOICE_CHANNEL',
  'GET_SELECTED_VOICE_CHANNEL',
  'SELECT_TEXT_CHANNEL',
  'GET_VOICE_SETTINGS',
  'SET_VOICE_SETTINGS_2',
  'SET_VOICE_SETTINGS',
  'CAPTURE_SHORTCUT',
  'SET_ACTIVITY',
  'SEND_ACTIVITY_JOIN_INVITE',
  'CLOSE_ACTIVITY_JOIN_REQUEST',
  'ACTIVITY_INVITE_USER',
  'ACCEPT_ACTIVITY_INVITE',
  'INVITE_BROWSER',
  'DEEP_LINK',
  'CONNECTIONS_CALLBACK',
  'BRAINTREE_POPUP_BRIDGE_CALLBACK',
  'GIFT_CODE_BROWSER',
  'GUILD_TEMPLATE_BROWSER',
  'OVERLAY',
  'BROWSER_HANDOFF',
  'SET_CERTIFIED_DEVICES',
  'GET_IMAGE',
  'CREATE_LOBBY',
  'UPDATE_LOBBY',
  'DELETE_LOBBY',
  'UPDATE_LOBBY_MEMBER',
  'CONNECT_TO_LOBBY',
  'DISCONNECT_FROM_LOBBY',
  'SEND_TO_LOBBY',
  'SEARCH_LOBBIES',
  'CONNECT_TO_LOBBY_VOICE',
  'DISCONNECT_FROM_LOBBY_VOICE',
  'SET_OVERLAY_LOCKED',
  'OPEN_OVERLAY_ACTIVITY_INVITE',
  'OPEN_OVERLAY_GUILD_INVITE',
  'OPEN_OVERLAY_VOICE_SETTINGS',
  'VALIDATE_APPLICATION',
  'GET_ENTITLEMENT_TICKET',
  'GET_APPLICATION_TICKET',
  'START_PURCHASE',
  'GET_SKUS',
  'GET_ENTITLEMENTS',
  'GET_NETWORKING_CONFIG',
  'NETWORKING_SYSTEM_METRICS',
  'NETWORKING_PEER_METRICS',
  'NETWORKING_CREATE_TOKEN',
  'SET_USER_ACHIEVEMENT',
  'GET_USER_ACHIEVEMENTS',
]);

var RPCEvents = keyMirror([
  'CURRENT_USER_UPDATE',
  'GUILD_STATUS',
  'GUILD_CREATE',
  'CHANNEL_CREATE',
  'RELATIONSHIP_UPDATE',
  'VOICE_CHANNEL_SELECT',
  'VOICE_STATE_CREATE',
  'VOICE_STATE_DELETE',
  'VOICE_STATE_UPDATE',
  'VOICE_SETTINGS_UPDATE',
  'VOICE_SETTINGS_UPDATE_2',
  'VOICE_CONNECTION_STATUS',
  'SPEAKING_START',
  'SPEAKING_STOP',
  'GAME_JOIN',
  'GAME_SPECTATE',
  'ACTIVITY_JOIN',
  'ACTIVITY_JOIN_REQUEST',
  'ACTIVITY_SPECTATE',
  'ACTIVITY_INVITE',
  'NOTIFICATION_CREATE',
  'MESSAGE_CREATE',
  'MESSAGE_UPDATE',
  'MESSAGE_DELETE',
  'LOBBY_DELETE',
  'LOBBY_UPDATE',
  'LOBBY_MEMBER_CONNECT',
  'LOBBY_MEMBER_DISCONNECT',
  'LOBBY_MEMBER_UPDATE',
  'LOBBY_MESSAGE',
  'CAPTURE_SHORTCUT_CHANGE',
  'OVERLAY',
  'OVERLAY_UPDATE',
  'ENTITLEMENT_CREATE',
  'ENTITLEMENT_DELETE',
  'USER_ACHIEVEMENT_UPDATE',
  'READY',
  'ERROR',
]);

var RPCErrors = {
  CAPTURE_SHORTCUT_ALREADY_LISTENING: 5004,
  GET_GUILD_TIMED_OUT: 5002,
  INVALID_ACTIVITY_JOIN_REQUEST: 4012,
  INVALID_ACTIVITY_SECRET: 5005,
  INVALID_CHANNEL: 4005,
  INVALID_CLIENTID: 4007,
  INVALID_COMMAND: 4002,
  INVALID_ENTITLEMENT: 4015,
  INVALID_EVENT: 4004,
  INVALID_GIFT_CODE: 4016,
  INVALID_GUILD: 4003,
  INVALID_INVITE: 4011,
  INVALID_LOBBY: 4013,
  INVALID_LOBBY_SECRET: 4014,
  INVALID_ORIGIN: 4008,
  INVALID_PAYLOAD: 4000,
  INVALID_PERMISSIONS: 4006,
  INVALID_TOKEN: 4009,
  INVALID_USER: 4010,
  LOBBY_FULL: 5007,
  NO_ELIGIBLE_ACTIVITY: 5006,
  OAUTH2_ERROR: 5000,
  PURCHASE_CANCELED: 5008,
  PURCHASE_ERROR: 5009,
  RATE_LIMITED: 5011,
  SELECT_CHANNEL_TIMED_OUT: 5001,
  SELECT_VOICE_FORCE_REQUIRED: 5003,
  SERVICE_UNAVAILABLE: 1001,
  TRANSACTION_ABORTED: 1002,
  UNAUTHORIZED_FOR_ACHIEVEMENT: 5010,
  UNKNOWN_ERROR: 1000,
};

var RPCCloseCodes = {
  CLOSE_NORMAL: 1000,
  CLOSE_UNSUPPORTED: 1003,
  CLOSE_ABNORMAL: 1006,
  INVALID_CLIENTID: 4000,
  INVALID_ORIGIN: 4001,
  RATELIMITED: 4002,
  TOKEN_REVOKED: 4003,
  INVALID_VERSION: 4004,
  INVALID_ENCODING: 4005,
};

var LobbyTypes = {
  PRIVATE: 1,
  PUBLIC: 2,
};

var RelationshipTypes = {
  NONE: 0,
  FRIEND: 1,
  BLOCKED: 2,
  PENDING_INCOMING: 3,
  PENDING_OUTGOING: 4,
  IMPLICIT: 5,
};

var constants = {
	browser: browser$1,
	RPCCommands: RPCCommands,
	RPCEvents: RPCEvents,
	RPCErrors: RPCErrors,
	RPCCloseCodes: RPCCloseCodes,
	LobbyTypes: LobbyTypes,
	RelationshipTypes: RelationshipTypes
};

const { browser: browser$2 } = constants;

// eslint-disable-next-line
const WebSocket = browser$2 ? window.WebSocket : require$$1;

const pack = (d) => JSON.stringify(d);
const unpack = (s) => JSON.parse(s);

class WebSocketTransport extends EventEmitter__default['default'] {
  constructor(client) {
    super();
    this.client = client;
    this.ws = null;
    this.tries = 0;
  }

  async connect(tries = this.tries) {
    if (this.connected) {
      return;
    }
    const port = 6463 + (tries % 10);
    this.hostAndPort = `127.0.0.1:${port}`;
    const ws = this.ws = new WebSocket(
      `ws://${this.hostAndPort}/?v=1&client_id=${this.client.clientId}`,
      {
        origin: this.client.options.origin,
      },
    );
    ws.onopen = this.onOpen.bind(this);
    ws.onclose = ws.onerror = this.onClose.bind(this);
    ws.onmessage = this.onMessage.bind(this);
  }

  send(data) {
    if (!this.ws) {
      return;
    }
    this.ws.send(pack(data));
  }

  close() {
    if (!this.ws) {
      return;
    }
    this.ws.close();
  }

  ping() {} // eslint-disable-line no-empty-function

  onMessage(event) {
    this.emit('message', unpack(event.data));
  }

  onOpen() {
    this.emit('open');
  }

  onClose(e) {
    try {
      this.ws.close();
    } catch (err) {} // eslint-disable-line no-empty
    const derr = e.code >= 4000 && e.code < 5000;
    if (!e.code || derr) {
      this.emit('close', e);
    }
    if (!derr) {
      // eslint-disable-next-line no-plusplus
      setTimeout(() => this.connect(e.code === 1006 ? ++this.tries : 0), 250);
    }
  }
}

var websocket = WebSocketTransport;

var transports = {
  ipc: ipc,
  websocket: websocket,
};

const { setTimeout: setTimeout$1, clearTimeout } = require$$0__default$1['default'];


const { RPCCommands: RPCCommands$1, RPCEvents: RPCEvents$1, RelationshipTypes: RelationshipTypes$1 } = constants;
const { pid: getPid, uuid: uuid$1 } = util;

function subKey(event, args) {
  return `${event}${JSON.stringify(args)}`;
}

/**
 * @typedef {RPCClientOptions}
 * @extends {ClientOptions}
 * @prop {string} transport RPC transport. one of `ipc` or `websocket`
 */

/**
 * The main hub for interacting with Discord RPC
 * @extends {BaseClient}
 */
class RPCClient extends EventEmitter__default['default'] {
  /**
   * @param {RPCClientOptions} [options] Options for the client.
   * You must provide a transport
   */
  constructor(options = {}) {
    super();

    this.options = options;

    this.accessToken = null;
    this.clientId = null;

    /**
     * Application used in this client
     * @type {?ClientApplication}
     */
    this.application = null;

    /**
     * User used in this application
     * @type {?User}
     */
    this.user = null;

    const Transport = transports[options.transport];
    if (!Transport) {
      throw new TypeError('RPC_INVALID_TRANSPORT', options.transport);
    }

    this.fetch = (method, path, { data, query } = {}) =>
      browser(`${this.fetch.endpoint}${path}${query ? new URLSearchParams(query) : ''}`, {
        method,
        body: data,
        headers: {
          Authorization: `Bearer ${this.accessToken}`,
        },
      }).then(async (r) => {
        const body = await r.json();
        if (!r.ok) {
          const e = new Error(r.status);
          e.body = body;
          throw e;
        }
        return body;
      });

    this.fetch.endpoint = 'https://discord.com/api';

    /**
     * Raw transport userd
     * @type {RPCTransport}
     * @private
     */
    this.transport = new Transport(this);
    this.transport.on('message', this._onRpcMessage.bind(this));

    /**
     * Map of nonces being expected from the transport
     * @type {Map}
     * @private
     */
    this._expecting = new Map();

    /**
     * Map of current subscriptions
     * @type {Map}
     * @private
     */
    this._subscriptions = new Map();

    this._connectPromise = undefined;
  }

  /**
   * Search and connect to RPC
   */
  connect(clientId) {
    if (this._connectPromise) {
      return this._connectPromise;
    }
    this._connectPromise = new Promise((resolve, reject) => {
      this.clientId = clientId;
      const timeout = setTimeout$1(() => reject(new Error('RPC_CONNECTION_TIMEOUT')), 10e3);
      timeout.unref();
      this.once('connected', () => {
        clearTimeout(timeout);
        resolve(this);
      });
      this.transport.once('close', () => {
        this._expecting.forEach((e) => {
          e.reject(new Error('connection closed'));
        });
        this.emit('disconnected');
        reject(new Error('connection closed'));
      });
      this.transport.connect().catch(reject);
    });
    return this._connectPromise;
  }

  /**
   * @typedef {RPCLoginOptions}
   * @param {string} clientId Client ID
   * @param {string} [clientSecret] Client secret
   * @param {string} [accessToken] Access token
   * @param {string} [rpcToken] RPC token
   * @param {string} [tokenEndpoint] Token endpoint
   * @param {string[]} [scopes] Scopes to authorize with
   */

  /**
   * Performs authentication flow. Automatically calls Client#connect if needed.
   * @param {RPCLoginOptions} options Options for authentication.
   * At least one property must be provided to perform login.
   * @example client.login({ clientId: '1234567', clientSecret: 'abcdef123' });
   * @returns {Promise<RPCClient>}
   */
  async login(options = {}) {
    let { clientId, accessToken } = options;
    await this.connect(clientId);
    if (!options.scopes) {
      this.emit('ready');
      return this;
    }
    if (!accessToken) {
      accessToken = await this.authorize(options);
    }
    return this.authenticate(accessToken);
  }

  /**
   * Request
   * @param {string} cmd Command
   * @param {Object} [args={}] Arguments
   * @param {string} [evt] Event
   * @returns {Promise}
   * @private
   */
  request(cmd, args, evt) {
    return new Promise((resolve, reject) => {
      const nonce = uuid$1();
      this.transport.send({ cmd, args, evt, nonce });
      this._expecting.set(nonce, { resolve, reject });
    });
  }

  /**
   * Message handler
   * @param {Object} message message
   * @private
   */
  _onRpcMessage(message) {
    if (message.cmd === RPCCommands$1.DISPATCH && message.evt === RPCEvents$1.READY) {
      if (message.data.user) {
        this.user = message.data.user;
      }
      this.emit('connected');
    } else if (this._expecting.has(message.nonce)) {
      const { resolve, reject } = this._expecting.get(message.nonce);
      if (message.evt === 'ERROR') {
        const e = new Error(message.data.message);
        e.code = message.data.code;
        e.data = message.data;
        reject(e);
      } else {
        resolve(message.data);
      }
      this._expecting.delete(message.nonce);
    } else {
      const subid = subKey(message.evt, message.args);
      if (!this._subscriptions.has(subid)) {
        return;
      }
      this._subscriptions.get(subid)(message.data);
    }
  }

  /**
   * Authorize
   * @param {Object} options options
   * @returns {Promise}
   * @private
   */
  async authorize({ scopes, clientSecret, rpcToken, redirectUri } = {}) {
    if (clientSecret && rpcToken === true) {
      const body = await this.fetch('POST', '/oauth2/token/rpc', {
        data: new URLSearchParams({
          client_id: this.clientId,
          client_secret: clientSecret,
        }),
      });
      rpcToken = body.rpc_token;
    }

    const { code } = await this.request('AUTHORIZE', {
      scopes,
      client_id: this.clientId,
      rpc_token: rpcToken,
    });

    const response = await this.fetch('POST', '/oauth2/token', {
      data: new URLSearchParams({
        client_id: this.clientId,
        client_secret: clientSecret,
        code,
        grant_type: 'authorization_code',
        redirect_uri: redirectUri,
      }),
    });

    return response.access_token;
  }

  /**
   * Authenticate
   * @param {string} accessToken access token
   * @returns {Promise}
   * @private
   */
  authenticate(accessToken) {
    return this.request('AUTHENTICATE', { access_token: accessToken })
      .then(({ application, user }) => {
        this.accessToken = accessToken;
        this.application = application;
        this.user = user;
        this.emit('ready');
        return this;
      });
  }


  /**
   * Fetch a guild
   * @param {Snowflake} id Guild ID
   * @param {number} [timeout] Timeout request
   * @returns {Promise<Guild>}
   */
  getGuild(id, timeout) {
    return this.request(RPCCommands$1.GET_GUILD, { guild_id: id, timeout });
  }

  /**
   * Fetch all guilds
   * @param {number} [timeout] Timeout request
   * @returns {Promise<Collection<Snowflake, Guild>>}
   */
  getGuilds(timeout) {
    return this.request(RPCCommands$1.GET_GUILDS, { timeout });
  }

  /**
   * Get a channel
   * @param {Snowflake} id Channel ID
   * @param {number} [timeout] Timeout request
   * @returns {Promise<Channel>}
   */
  getChannel(id, timeout) {
    return this.request(RPCCommands$1.GET_CHANNEL, { channel_id: id, timeout });
  }

  /**
   * Get all channels
   * @param {Snowflake} [id] Guild ID
   * @param {number} [timeout] Timeout request
   * @returns {Promise<Collection<Snowflake, Channel>>}
   */
  async getChannels(id, timeout) {
    const { channels } = await this.request(RPCCommands$1.GET_CHANNELS, {
      timeout,
      guild_id: id,
    });
    return channels;
  }

  /**
   * @typedef {CertifiedDevice}
   * @prop {string} type One of `AUDIO_INPUT`, `AUDIO_OUTPUT`, `VIDEO_INPUT`
   * @prop {string} uuid This device's Windows UUID
   * @prop {object} vendor Vendor information
   * @prop {string} vendor.name Vendor's name
   * @prop {string} vendor.url Vendor's url
   * @prop {object} model Model information
   * @prop {string} model.name Model's name
   * @prop {string} model.url Model's url
   * @prop {string[]} related Array of related product's Windows UUIDs
   * @prop {boolean} echoCancellation If the device has echo cancellation
   * @prop {boolean} noiseSuppression If the device has noise suppression
   * @prop {boolean} automaticGainControl If the device has automatic gain control
   * @prop {boolean} hardwareMute If the device has a hardware mute
   */

  /**
   * Tell discord which devices are certified
   * @param {CertifiedDevice[]} devices Certified devices to send to discord
   * @returns {Promise}
   */
  setCertifiedDevices(devices) {
    return this.request(RPCCommands$1.SET_CERTIFIED_DEVICES, {
      devices: devices.map((d) => ({
        type: d.type,
        id: d.uuid,
        vendor: d.vendor,
        model: d.model,
        related: d.related,
        echo_cancellation: d.echoCancellation,
        noise_suppression: d.noiseSuppression,
        automatic_gain_control: d.automaticGainControl,
        hardware_mute: d.hardwareMute,
      })),
    });
  }

  /**
   * @typedef {UserVoiceSettings}
   * @prop {Snowflake} id ID of the user these settings apply to
   * @prop {?Object} [pan] Pan settings, an object with `left` and `right` set between
   * 0.0 and 1.0, inclusive
   * @prop {?number} [volume=100] The volume
   * @prop {bool} [mute] If the user is muted
   */

  /**
   * Set the voice settings for a uer, by id
   * @param {Snowflake} id ID of the user to set
   * @param {UserVoiceSettings} settings Settings
   * @returns {Promise}
   */
  setUserVoiceSettings(id, settings) {
    return this.request(RPCCommands$1.SET_USER_VOICE_SETTINGS, {
      user_id: id,
      pan: settings.pan,
      mute: settings.mute,
      volume: settings.volume,
    });
  }

  /**
   * Move the user to a voice channel
   * @param {Snowflake} id ID of the voice channel
   * @param {Object} [options] Options
   * @param {number} [options.timeout] Timeout for the command
   * @param {boolean} [options.force] Force this move. This should only be done if you
   * have explicit permission from the user.
   * @returns {Promise}
   */
  selectVoiceChannel(id, { timeout, force = false } = {}) {
    return this.request(RPCCommands$1.SELECT_VOICE_CHANNEL, { channel_id: id, timeout, force });
  }

  /**
   * Move the user to a text channel
   * @param {Snowflake} id ID of the voice channel
   * @param {Object} [options] Options
   * @param {number} [options.timeout] Timeout for the command
   * @param {boolean} [options.force] Force this move. This should only be done if you
   * have explicit permission from the user.
   * @returns {Promise}
   */
  selectTextChannel(id, { timeout, force = false } = {}) {
    return this.request(RPCCommands$1.SELECT_TEXT_CHANNEL, { channel_id: id, timeout, force });
  }

  /**
   * Get current voice settings
   * @returns {Promise}
   */
  getVoiceSettings() {
    return this.request(RPCCommands$1.GET_VOICE_SETTINGS)
      .then((s) => ({
        automaticGainControl: s.automatic_gain_control,
        echoCancellation: s.echo_cancellation,
        noiseSuppression: s.noise_suppression,
        qos: s.qos,
        silenceWarning: s.silence_warning,
        deaf: s.deaf,
        mute: s.mute,
        input: {
          availableDevices: s.input.available_devices,
          device: s.input.device_id,
          volume: s.input.volume,
        },
        output: {
          availableDevices: s.output.available_devices,
          device: s.output.device_id,
          volume: s.output.volume,
        },
        mode: {
          type: s.mode.type,
          autoThreshold: s.mode.auto_threshold,
          threshold: s.mode.threshold,
          shortcut: s.mode.shortcut,
          delay: s.mode.delay,
        },
      }));
  }

  /**
   * Set current voice settings, overriding the current settings until this session disconnects.
   * This also locks the settings for any other rpc sessions which may be connected.
   * @param {Object} args Settings
   * @returns {Promise}
   */
  setVoiceSettings(args) {
    return this.request(RPCCommands$1.SET_VOICE_SETTINGS, {
      automatic_gain_control: args.automaticGainControl,
      echo_cancellation: args.echoCancellation,
      noise_suppression: args.noiseSuppression,
      qos: args.qos,
      silence_warning: args.silenceWarning,
      deaf: args.deaf,
      mute: args.mute,
      input: args.input ? {
        device_id: args.input.device,
        volume: args.input.volume,
      } : undefined,
      output: args.output ? {
        device_id: args.output.device,
        volume: args.output.volume,
      } : undefined,
      mode: args.mode ? {
        mode: args.mode.type,
        auto_threshold: args.mode.autoThreshold,
        threshold: args.mode.threshold,
        shortcut: args.mode.shortcut,
        delay: args.mode.delay,
      } : undefined,
    });
  }

  /**
   * Capture a shortcut using the client
   * The callback takes (key, stop) where `stop` is a function that will stop capturing.
   * This `stop` function must be called before disconnecting or else the user will have
   * to restart their client.
   * @param {Function} callback Callback handling keys
   * @returns {Promise<Function>}
   */
  captureShortcut(callback) {
    const subid = subKey(RPCEvents$1.CAPTURE_SHORTCUT_CHANGE);
    const stop = () => {
      this._subscriptions.delete(subid);
      return this.request(RPCCommands$1.CAPTURE_SHORTCUT, { action: 'STOP' });
    };
    this._subscriptions.set(subid, ({ shortcut }) => {
      callback(shortcut, stop);
    });
    return this.request(RPCCommands$1.CAPTURE_SHORTCUT, { action: 'START' })
      .then(() => stop);
  }

  /**
   * Sets the presence for the logged in user.
   * @param {object} args The rich presence to pass.
   * @param {number} [pid] The application's process ID. Defaults to the executing process' PID.
   * @returns {Promise}
   */
  setActivity(args = {}, pid = getPid()) {
    let timestamps;
    let assets;
    let party;
    let secrets;
    if (args.startTimestamp || args.endTimestamp) {
      timestamps = {
        start: args.startTimestamp,
        end: args.endTimestamp,
      };
      if (timestamps.start instanceof Date) {
        timestamps.start = Math.round(timestamps.start.getTime());
      }
      if (timestamps.end instanceof Date) {
        timestamps.end = Math.round(timestamps.end.getTime());
      }
      if (timestamps.start > 2147483647000) {
        throw new RangeError('timestamps.start must fit into a unix timestamp');
      }
      if (timestamps.end > 2147483647000) {
        throw new RangeError('timestamps.end must fit into a unix timestamp');
      }
    }
    if (
      args.largeImageKey || args.largeImageText
      || args.smallImageKey || args.smallImageText
    ) {
      assets = {
        large_image: args.largeImageKey,
        large_text: args.largeImageText,
        small_image: args.smallImageKey,
        small_text: args.smallImageText,
      };
    }
    if (args.partySize || args.partyId || args.partyMax) {
      party = { id: args.partyId };
      if (args.partySize || args.partyMax) {
        party.size = [args.partySize, args.partyMax];
      }
    }
    if (args.matchSecret || args.joinSecret || args.spectateSecret) {
      secrets = {
        match: args.matchSecret,
        join: args.joinSecret,
        spectate: args.spectateSecret,
      };
    }

    return this.request(RPCCommands$1.SET_ACTIVITY, {
      pid,
      activity: {
        state: args.state,
        details: args.details,
        timestamps,
        assets,
        party,
        secrets,
        instance: !!args.instance,
      },
    });
  }

  /**
   * Clears the currently set presence, if any. This will hide the "Playing X" message
   * displayed below the user's name.
   * @param {number} [pid] The application's process ID. Defaults to the executing process' PID.
   * @returns {Promise}
   */
  clearActivity(pid = getPid()) {
    return this.request(RPCCommands$1.SET_ACTIVITY, {
      pid,
    });
  }

  /**
   * Invite a user to join the game the RPC user is currently playing
   * @param {User} user The user to invite
   * @returns {Promise}
   */
  sendJoinInvite(user) {
    return this.request(RPCCommands$1.SEND_ACTIVITY_JOIN_INVITE, {
      user_id: user.id || user,
    });
  }

  /**
   * Request to join the game the user is playing
   * @param {User} user The user whose game you want to request to join
   * @returns {Promise}
   */
  sendJoinRequest(user) {
    return this.request(RPCCommands$1.SEND_ACTIVITY_JOIN_REQUEST, {
      user_id: user.id || user,
    });
  }

  /**
   * Reject a join request from a user
   * @param {User} user The user whose request you wish to reject
   * @returns {Promise}
   */
  closeJoinRequest(user) {
    return this.request(RPCCommands$1.CLOSE_ACTIVITY_JOIN_REQUEST, {
      user_id: user.id || user,
    });
  }

  createLobby(type, capacity, metadata) {
    return this.request(RPCCommands$1.CREATE_LOBBY, {
      type,
      capacity,
      metadata,
    });
  }

  updateLobby(lobby, { type, owner, capacity, metadata } = {}) {
    return this.request(RPCCommands$1.UPDATE_LOBBY, {
      id: lobby.id || lobby,
      type,
      owner_id: (owner && owner.id) || owner,
      capacity,
      metadata,
    });
  }

  deleteLobby(lobby) {
    return this.request(RPCCommands$1.DELETE_LOBBY, {
      id: lobby.id || lobby,
    });
  }

  connectToLobby(id, secret) {
    return this.request(RPCCommands$1.CONNECT_TO_LOBBY, {
      id,
      secret,
    });
  }

  sendToLobby(lobby, data) {
    return this.request(RPCCommands$1.SEND_TO_LOBBY, {
      id: lobby.id || lobby,
      data,
    });
  }

  disconnectFromLobby(lobby) {
    return this.request(RPCCommands$1.DISCONNECT_FROM_LOBBY, {
      id: lobby.id || lobby,
    });
  }

  updateLobbyMember(lobby, user, metadata) {
    return this.request(RPCCommands$1.UPDATE_LOBBY_MEMBER, {
      lobby_id: lobby.id || lobby,
      user_id: user.id || user,
      metadata,
    });
  }

  getRelationships() {
    const types = Object.keys(RelationshipTypes$1);
    return this.request(RPCCommands$1.GET_RELATIONSHIPS)
      .then((o) => o.relationships.map((r) => ({
        ...r,
        type: types[r.type],
      })));
  }

  /**
   * Subscribe to an event
   * @param {string} event Name of event e.g. `MESSAGE_CREATE`
   * @param {Object} [args] Args for event e.g. `{ channel_id: '1234' }`
   * @param {Function} callback Callback when an event for the subscription is triggered
   * @returns {Promise<Object>}
   */
  subscribe(event, args, callback) {
    if (!callback && typeof args === 'function') {
      callback = args;
      args = undefined;
    }
    return this.request(RPCCommands$1.SUBSCRIBE, args, event).then(() => {
      const subid = subKey(event, args);
      this._subscriptions.set(subid, callback);
      return {
        unsubscribe: () => this.request(RPCCommands$1.UNSUBSCRIBE, args, event)
          .then(() => this._subscriptions.delete(subid)),
      };
    });
  }

  /**
   * Destroy the client
   */
  async destroy() {
    this.transport.close();
  }
}

var client = RPCClient;

var src = {
  Client: client,
  register(id) {
    return util.register(`discord-${id}`);
  },
};

var Logger = /** @class */ (function () {
    function Logger() {
        this.plugin = this.plugin;
    }
    Logger.prototype.log = function (message, showPopups) {
        if (showPopups) {
            new obsidian.Notice(message);
        }
        console.log("discordrpc: " + message);
    };
    Logger.prototype.logIgnoreNoNotice = function (message) {
        new obsidian.Notice(message);
        console.log("discordrpc: " + message);
    };
    return Logger;
}());

var DiscordRPCSettings = /** @class */ (function () {
    function DiscordRPCSettings() {
        this.showVaultName = true;
        this.showCurrentFileName = true;
        this.showPopups = true;
        this.customVaultName = "";
        this.showFileExtension = false;
        this.useLoadedTime = false;
    }
    return DiscordRPCSettings;
}());
var PluginState;
(function (PluginState) {
    PluginState[PluginState["connected"] = 0] = "connected";
    PluginState[PluginState["connecting"] = 1] = "connecting";
    PluginState[PluginState["disconnected"] = 2] = "disconnected";
})(PluginState || (PluginState = {}));

var DiscordRPCSettingsTab = /** @class */ (function (_super) {
    __extends(DiscordRPCSettingsTab, _super);
    function DiscordRPCSettingsTab() {
        var _this = _super !== null && _super.apply(this, arguments) || this;
        _this.logger = new Logger();
        return _this;
    }
    DiscordRPCSettingsTab.prototype.display = function () {
        var _this = this;
        var containerEl = this.containerEl;
        var plugin = this.plugin;
        containerEl.empty();
        containerEl.createEl("h2", { text: "Discord Rich Presence Settings" });
        containerEl.createEl("h3", { text: "Vault Name Settings" });
        new obsidian.Setting(containerEl)
            .setName("Show Vault Name")
            .setDesc("Enable this to show the name of the vault you are working with.")
            .addToggle(function (boolean) {
            return boolean.setValue(plugin.settings.showVaultName).onChange(function (value) {
                plugin.settings.showVaultName = value;
                plugin.saveData(plugin.settings);
                if (boolean.getValue()) {
                    _this.logger.logIgnoreNoNotice("Vault Name is now Visable");
                }
                else {
                    _this.logger.logIgnoreNoNotice("Vault Name is no longer Visable");
                }
                plugin.setActivity(_this.app.vault.getName(), plugin.currentFile.basename, plugin.currentFile.extension);
            });
        });
        new obsidian.Setting(containerEl)
            .setName("Set Custom Vault Name")
            .setDesc("Change the vault name shown publicly. Leave blank to use your actual vault name.")
            .addText(function (text) {
            return text.setValue(plugin.settings.customVaultName).onChange(function (value) {
                plugin.settings.customVaultName = value;
                plugin.saveData(plugin.settings);
                plugin.setActivity(_this.app.vault.getName(), plugin.currentFile.basename, plugin.currentFile.extension);
            });
        });
        containerEl.createEl("h3", { text: "File Name Settings" });
        new obsidian.Setting(containerEl)
            .setName("Show Current File Name")
            .setDesc("Enable this to show the name of the file you are working on.")
            .addToggle(function (boolean) {
            return boolean
                .setValue(plugin.settings.showCurrentFileName)
                .onChange(function (value) {
                plugin.settings.showCurrentFileName = value;
                plugin.saveData(plugin.settings);
                if (boolean.getValue()) {
                    _this.logger.logIgnoreNoNotice("File Name is now Visable");
                }
                else {
                    _this.logger.logIgnoreNoNotice("File Name is no longer Visable");
                }
                plugin.setActivity(_this.app.vault.getName(), plugin.currentFile.basename, plugin.currentFile.extension);
            });
        });
        new obsidian.Setting(containerEl)
            .setName("Show File Extension")
            .setDesc("Enable this to show file extension.")
            .addToggle(function (boolean) {
            return boolean
                .setValue(plugin.settings.showFileExtension)
                .onChange(function (value) {
                plugin.settings.showFileExtension = value;
                plugin.saveData(plugin.settings);
                plugin.setActivity(_this.app.vault.getName(), plugin.currentFile.basename, plugin.currentFile.extension);
            });
        });
        containerEl.createEl("h3", { text: "Time Settings" });
        new obsidian.Setting(containerEl)
            .setName("Use Obsidian Total Time")
            .setDesc("Enable to use the total time you have been using Obsidian, instead of the time spent editing a single file.")
            .addToggle(function (boolean) {
            boolean.setValue(plugin.settings.useLoadedTime).onChange(function (value) {
                plugin.settings.useLoadedTime = value;
                plugin.saveData(plugin.settings);
                plugin.setActivity(_this.app.vault.getName(), plugin.currentFile.basename, plugin.currentFile.extension);
            });
        });
        containerEl.createEl("h3", { text: "Notice Settings" });
        new obsidian.Setting(containerEl)
            .setName("Show Notices")
            .setDesc("Enable this to show connection Notices.")
            .addToggle(function (boolean) {
            return boolean.setValue(plugin.settings.showPopups).onChange(function (value) {
                plugin.settings.showPopups = value;
                plugin.saveData(plugin.settings);
                if (boolean.getValue()) {
                    _this.logger.logIgnoreNoNotice("Notices Enabled");
                }
                else {
                    _this.logger.logIgnoreNoNotice("Notices Disabled");
                }
                plugin.setActivity(_this.app.vault.getName(), plugin.currentFile.basename, plugin.currentFile.extension);
            });
        });
    };
    return DiscordRPCSettingsTab;
}(obsidian.PluginSettingTab));

var StatusBar = /** @class */ (function () {
    function StatusBar(statusBarEl) {
        this.statusBarEl = statusBarEl;
    }
    StatusBar.prototype.displayState = function (state) {
        switch (state) {
            case PluginState.connected:
                this.displayConnected(2000);
                break;
            case PluginState.connecting:
                this.statusBarEl.setText("Connecting to Discord...");
                break;
            case PluginState.disconnected:
                this.statusBarEl.setText("\uD83D\uDDD8 Reconnect to Discord");
                break;
        }
    };
    StatusBar.prototype.displayConnected = function (timeout) {
        var _this = this;
        this.statusBarEl.setText("\uD83C\uDF0D Connected to Discord");
        if (timeout && timeout > 0) {
            window.setTimeout(function () {
                _this.statusBarEl.setText("\uD83C\uDF0D");
            }, timeout);
        }
    };
    return StatusBar;
}());

var ObsidianDiscordRPC = /** @class */ (function (_super) {
    __extends(ObsidianDiscordRPC, _super);
    function ObsidianDiscordRPC() {
        var _this = _super !== null && _super.apply(this, arguments) || this;
        _this.logger = new Logger();
        return _this;
    }
    ObsidianDiscordRPC.prototype.setState = function (state) {
        this.state = state;
    };
    ObsidianDiscordRPC.prototype.getState = function () {
        return this.state;
    };
    ObsidianDiscordRPC.prototype.getApp = function () {
        return this.app;
    };
    ObsidianDiscordRPC.prototype.getPluginManifest = function () {
        return this.manifest;
    };
    ObsidianDiscordRPC.prototype.onload = function () {
        return __awaiter(this, void 0, void 0, function () {
            var statusBarEl, _a, activeLeaf, files;
            var _this = this;
            return __generator(this, function (_b) {
                switch (_b.label) {
                    case 0:
                        this.loadedTime = new Date();
                        statusBarEl = this.addStatusBarItem();
                        this.statusBar = new StatusBar(statusBarEl);
                        _a = this;
                        return [4 /*yield*/, this.loadData()];
                    case 1:
                        _a.settings = (_b.sent()) || new DiscordRPCSettings();
                        this.registerEvent(this.app.workspace.on("file-open", this.onFileOpen, this));
                        this.registerDomEvent(statusBarEl, "click", function () { return __awaiter(_this, void 0, void 0, function () {
                            return __generator(this, function (_a) {
                                switch (_a.label) {
                                    case 0:
                                        if (!(this.getState() == PluginState.disconnected)) return [3 /*break*/, 2];
                                        return [4 /*yield*/, this.connectDiscord()];
                                    case 1:
                                        _a.sent();
                                        _a.label = 2;
                                    case 2: return [2 /*return*/];
                                }
                            });
                        }); });
                        this.addSettingTab(new DiscordRPCSettingsTab(this.app, this));
                        this.addCommand({
                            id: "reconnect-discord",
                            name: "Reconnect to Discord",
                            callback: function () { return __awaiter(_this, void 0, void 0, function () { return __generator(this, function (_a) {
                                switch (_a.label) {
                                    case 0: return [4 /*yield*/, this.connectDiscord()];
                                    case 1: return [2 /*return*/, _a.sent()];
                                }
                            }); }); },
                        });
                        return [4 /*yield*/, this.connectDiscord()];
                    case 2:
                        _b.sent();
                        activeLeaf = this.app.workspace.activeLeaf;
                        files = this.app.vault.getMarkdownFiles();
                        files.forEach(function (file) {
                            if (file.basename === activeLeaf.getDisplayText()) {
                                _this.onFileOpen(file);
                            }
                        });
                        return [2 /*return*/];
                }
            });
        });
    };
    ObsidianDiscordRPC.prototype.onFileOpen = function (file) {
        return __awaiter(this, void 0, void 0, function () {
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0:
                        this.currentFile = file;
                        if (!(this.getState() === PluginState.connected)) return [3 /*break*/, 2];
                        return [4 /*yield*/, this.setActivity(this.app.vault.getName(), file.basename, file.extension)];
                    case 1:
                        _a.sent();
                        _a.label = 2;
                    case 2: return [2 /*return*/];
                }
            });
        });
    };
    ObsidianDiscordRPC.prototype.onunload = function () {
        return __awaiter(this, void 0, void 0, function () {
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0: return [4 /*yield*/, this.saveData(this.settings)];
                    case 1:
                        _a.sent();
                        this.rpc.clearActivity();
                        this.rpc.destroy();
                        return [2 /*return*/];
                }
            });
        });
    };
    ObsidianDiscordRPC.prototype.connectDiscord = function () {
        return __awaiter(this, void 0, void 0, function () {
            var error_1;
            var _this = this;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0:
                        this.rpc = new src.Client({
                            transport: "ipc",
                        });
                        this.setState(PluginState.connecting);
                        this.statusBar.displayState(this.getState());
                        this.rpc.once("ready", function () {
                            _this.setState(PluginState.connected);
                            _this.statusBar.displayState(_this.getState());
                            _this.logger.log("Connected to Discord", _this.settings.showPopups);
                        });
                        _a.label = 1;
                    case 1:
                        _a.trys.push([1, 4, , 5]);
                        return [4 /*yield*/, this.rpc.login({
                                clientId: "763813185022197831",
                            })];
                    case 2:
                        _a.sent();
                        return [4 /*yield*/, this.setActivity(this.app.vault.getName(), "...", "")];
                    case 3:
                        _a.sent();
                        return [3 /*break*/, 5];
                    case 4:
                        error_1 = _a.sent();
                        this.setState(PluginState.disconnected);
                        this.statusBar.displayState(this.getState());
                        this.logger.log("Failed to connect to Discord", this.settings.showPopups);
                        return [3 /*break*/, 5];
                    case 5: return [2 /*return*/];
                }
            });
        });
    };
    ObsidianDiscordRPC.prototype.setActivity = function (vaultName, fileName, fileExtension) {
        return __awaiter(this, void 0, void 0, function () {
            var vault, file, date;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0:
                        if (!(this.getState() === PluginState.connected)) return [3 /*break*/, 8];
                        vault = void 0;
                        if (this.settings.customVaultName === "") {
                            vault = vaultName;
                        }
                        else {
                            vault = this.settings.customVaultName;
                        }
                        file = void 0;
                        if (this.settings.showFileExtension) {
                            file = fileName + "." + fileExtension;
                        }
                        else {
                            file = fileName;
                        }
                        date = void 0;
                        if (this.settings.useLoadedTime) {
                            date = this.loadedTime;
                        }
                        else {
                            date = new Date();
                        }
                        if (!(this.settings.showVaultName && this.settings.showCurrentFileName)) return [3 /*break*/, 2];
                        return [4 /*yield*/, this.rpc.setActivity({
                                details: "Editing " + file,
                                state: "Vault: " + vault,
                                startTimestamp: date,
                                largeImageKey: "logo",
                                largeImageText: "Obsidian",
                            })];
                    case 1:
                        _a.sent();
                        return [3 /*break*/, 8];
                    case 2:
                        if (!this.settings.showVaultName) return [3 /*break*/, 4];
                        return [4 /*yield*/, this.rpc.setActivity({
                                state: "Vault: " + vault,
                                startTimestamp: date,
                                largeImageKey: "logo",
                                largeImageText: "Obsidian",
                            })];
                    case 3:
                        _a.sent();
                        return [3 /*break*/, 8];
                    case 4:
                        if (!this.settings.showCurrentFileName) return [3 /*break*/, 6];
                        return [4 /*yield*/, this.rpc.setActivity({
                                details: "Editing " + file,
                                startTimestamp: date,
                                largeImageKey: "logo",
                                largeImageText: "Obsidian",
                            })];
                    case 5:
                        _a.sent();
                        return [3 /*break*/, 8];
                    case 6: return [4 /*yield*/, this.rpc.setActivity({
                            startTimestamp: new Date(),
                            largeImageKey: "logo",
                            largeImageText: "Obsidian",
                        })];
                    case 7:
                        _a.sent();
                        _a.label = 8;
                    case 8: return [2 /*return*/];
                }
            });
        });
    };
    return ObsidianDiscordRPC;
}(obsidian.Plugin));

module.exports = ObsidianDiscordRPC;
//# sourceMappingURL=data:application/json;charset=utf-8;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoibWFpbi5qcyIsInNvdXJjZXMiOlsiLi4vbm9kZV9tb2R1bGVzL3RzbGliL3RzbGliLmVzNi5qcyIsIi4uL25vZGVfbW9kdWxlcy9kaXNjb3JkLXJwYy9zcmMvdXRpbC5qcyIsIi4uL25vZGVfbW9kdWxlcy9ub2RlLWZldGNoL2Jyb3dzZXIuanMiLCIuLi9ub2RlX21vZHVsZXMvZGlzY29yZC1ycGMvc3JjL3RyYW5zcG9ydHMvaXBjLmpzIiwiLi4vbm9kZV9tb2R1bGVzL2Rpc2NvcmQtcnBjL3NyYy9jb25zdGFudHMuanMiLCIuLi9ub2RlX21vZHVsZXMvZGlzY29yZC1ycGMvc3JjL3RyYW5zcG9ydHMvd2Vic29ja2V0LmpzIiwiLi4vbm9kZV9tb2R1bGVzL2Rpc2NvcmQtcnBjL3NyYy90cmFuc3BvcnRzL2luZGV4LmpzIiwiLi4vbm9kZV9tb2R1bGVzL2Rpc2NvcmQtcnBjL3NyYy9jbGllbnQuanMiLCIuLi9ub2RlX21vZHVsZXMvZGlzY29yZC1ycGMvc3JjL2luZGV4LmpzIiwiLi4vc3JjL2xvZ2dlci50cyIsIi4uL3NyYy9zZXR0aW5ncy9zZXR0aW5ncy50cyIsIi4uL3NyYy9zZXR0aW5ncy9zZXR0aW5ncy10YWIudHMiLCIuLi9zcmMvc3RhdHVzLWJhci50cyIsIi4uL3NyYy9tYWluLnRzIl0sInNvdXJjZXNDb250ZW50IjpbIi8qISAqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKlxyXG5Db3B5cmlnaHQgKGMpIE1pY3Jvc29mdCBDb3Jwb3JhdGlvbi5cclxuXHJcblBlcm1pc3Npb24gdG8gdXNlLCBjb3B5LCBtb2RpZnksIGFuZC9vciBkaXN0cmlidXRlIHRoaXMgc29mdHdhcmUgZm9yIGFueVxyXG5wdXJwb3NlIHdpdGggb3Igd2l0aG91dCBmZWUgaXMgaGVyZWJ5IGdyYW50ZWQuXHJcblxyXG5USEUgU09GVFdBUkUgSVMgUFJPVklERUQgXCJBUyBJU1wiIEFORCBUSEUgQVVUSE9SIERJU0NMQUlNUyBBTEwgV0FSUkFOVElFUyBXSVRIXHJcblJFR0FSRCBUTyBUSElTIFNPRlRXQVJFIElOQ0xVRElORyBBTEwgSU1QTElFRCBXQVJSQU5USUVTIE9GIE1FUkNIQU5UQUJJTElUWVxyXG5BTkQgRklUTkVTUy4gSU4gTk8gRVZFTlQgU0hBTEwgVEhFIEFVVEhPUiBCRSBMSUFCTEUgRk9SIEFOWSBTUEVDSUFMLCBESVJFQ1QsXHJcbklORElSRUNULCBPUiBDT05TRVFVRU5USUFMIERBTUFHRVMgT1IgQU5ZIERBTUFHRVMgV0hBVFNPRVZFUiBSRVNVTFRJTkcgRlJPTVxyXG5MT1NTIE9GIFVTRSwgREFUQSBPUiBQUk9GSVRTLCBXSEVUSEVSIElOIEFOIEFDVElPTiBPRiBDT05UUkFDVCwgTkVHTElHRU5DRSBPUlxyXG5PVEhFUiBUT1JUSU9VUyBBQ1RJT04sIEFSSVNJTkcgT1VUIE9GIE9SIElOIENPTk5FQ1RJT04gV0lUSCBUSEUgVVNFIE9SXHJcblBFUkZPUk1BTkNFIE9GIFRISVMgU09GVFdBUkUuXHJcbioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqICovXHJcbi8qIGdsb2JhbCBSZWZsZWN0LCBQcm9taXNlICovXHJcblxyXG52YXIgZXh0ZW5kU3RhdGljcyA9IGZ1bmN0aW9uKGQsIGIpIHtcclxuICAgIGV4dGVuZFN0YXRpY3MgPSBPYmplY3Quc2V0UHJvdG90eXBlT2YgfHxcclxuICAgICAgICAoeyBfX3Byb3RvX186IFtdIH0gaW5zdGFuY2VvZiBBcnJheSAmJiBmdW5jdGlvbiAoZCwgYikgeyBkLl9fcHJvdG9fXyA9IGI7IH0pIHx8XHJcbiAgICAgICAgZnVuY3Rpb24gKGQsIGIpIHsgZm9yICh2YXIgcCBpbiBiKSBpZiAoT2JqZWN0LnByb3RvdHlwZS5oYXNPd25Qcm9wZXJ0eS5jYWxsKGIsIHApKSBkW3BdID0gYltwXTsgfTtcclxuICAgIHJldHVybiBleHRlbmRTdGF0aWNzKGQsIGIpO1xyXG59O1xyXG5cclxuZXhwb3J0IGZ1bmN0aW9uIF9fZXh0ZW5kcyhkLCBiKSB7XHJcbiAgICBleHRlbmRTdGF0aWNzKGQsIGIpO1xyXG4gICAgZnVuY3Rpb24gX18oKSB7IHRoaXMuY29uc3RydWN0b3IgPSBkOyB9XHJcbiAgICBkLnByb3RvdHlwZSA9IGIgPT09IG51bGwgPyBPYmplY3QuY3JlYXRlKGIpIDogKF9fLnByb3RvdHlwZSA9IGIucHJvdG90eXBlLCBuZXcgX18oKSk7XHJcbn1cclxuXHJcbmV4cG9ydCB2YXIgX19hc3NpZ24gPSBmdW5jdGlvbigpIHtcclxuICAgIF9fYXNzaWduID0gT2JqZWN0LmFzc2lnbiB8fCBmdW5jdGlvbiBfX2Fzc2lnbih0KSB7XHJcbiAgICAgICAgZm9yICh2YXIgcywgaSA9IDEsIG4gPSBhcmd1bWVudHMubGVuZ3RoOyBpIDwgbjsgaSsrKSB7XHJcbiAgICAgICAgICAgIHMgPSBhcmd1bWVudHNbaV07XHJcbiAgICAgICAgICAgIGZvciAodmFyIHAgaW4gcykgaWYgKE9iamVjdC5wcm90b3R5cGUuaGFzT3duUHJvcGVydHkuY2FsbChzLCBwKSkgdFtwXSA9IHNbcF07XHJcbiAgICAgICAgfVxyXG4gICAgICAgIHJldHVybiB0O1xyXG4gICAgfVxyXG4gICAgcmV0dXJuIF9fYXNzaWduLmFwcGx5KHRoaXMsIGFyZ3VtZW50cyk7XHJcbn1cclxuXHJcbmV4cG9ydCBmdW5jdGlvbiBfX3Jlc3QocywgZSkge1xyXG4gICAgdmFyIHQgPSB7fTtcclxuICAgIGZvciAodmFyIHAgaW4gcykgaWYgKE9iamVjdC5wcm90b3R5cGUuaGFzT3duUHJvcGVydHkuY2FsbChzLCBwKSAmJiBlLmluZGV4T2YocCkgPCAwKVxyXG4gICAgICAgIHRbcF0gPSBzW3BdO1xyXG4gICAgaWYgKHMgIT0gbnVsbCAmJiB0eXBlb2YgT2JqZWN0LmdldE93blByb3BlcnR5U3ltYm9scyA9PT0gXCJmdW5jdGlvblwiKVxyXG4gICAgICAgIGZvciAodmFyIGkgPSAwLCBwID0gT2JqZWN0LmdldE93blByb3BlcnR5U3ltYm9scyhzKTsgaSA8IHAubGVuZ3RoOyBpKyspIHtcclxuICAgICAgICAgICAgaWYgKGUuaW5kZXhPZihwW2ldKSA8IDAgJiYgT2JqZWN0LnByb3RvdHlwZS5wcm9wZXJ0eUlzRW51bWVyYWJsZS5jYWxsKHMsIHBbaV0pKVxyXG4gICAgICAgICAgICAgICAgdFtwW2ldXSA9IHNbcFtpXV07XHJcbiAgICAgICAgfVxyXG4gICAgcmV0dXJuIHQ7XHJcbn1cclxuXHJcbmV4cG9ydCBmdW5jdGlvbiBfX2RlY29yYXRlKGRlY29yYXRvcnMsIHRhcmdldCwga2V5LCBkZXNjKSB7XHJcbiAgICB2YXIgYyA9IGFyZ3VtZW50cy5sZW5ndGgsIHIgPSBjIDwgMyA/IHRhcmdldCA6IGRlc2MgPT09IG51bGwgPyBkZXNjID0gT2JqZWN0LmdldE93blByb3BlcnR5RGVzY3JpcHRvcih0YXJnZXQsIGtleSkgOiBkZXNjLCBkO1xyXG4gICAgaWYgKHR5cGVvZiBSZWZsZWN0ID09PSBcIm9iamVjdFwiICYmIHR5cGVvZiBSZWZsZWN0LmRlY29yYXRlID09PSBcImZ1bmN0aW9uXCIpIHIgPSBSZWZsZWN0LmRlY29yYXRlKGRlY29yYXRvcnMsIHRhcmdldCwga2V5LCBkZXNjKTtcclxuICAgIGVsc2UgZm9yICh2YXIgaSA9IGRlY29yYXRvcnMubGVuZ3RoIC0gMTsgaSA+PSAwOyBpLS0pIGlmIChkID0gZGVjb3JhdG9yc1tpXSkgciA9IChjIDwgMyA/IGQocikgOiBjID4gMyA/IGQodGFyZ2V0LCBrZXksIHIpIDogZCh0YXJnZXQsIGtleSkpIHx8IHI7XHJcbiAgICByZXR1cm4gYyA+IDMgJiYgciAmJiBPYmplY3QuZGVmaW5lUHJvcGVydHkodGFyZ2V0LCBrZXksIHIpLCByO1xyXG59XHJcblxyXG5leHBvcnQgZnVuY3Rpb24gX19wYXJhbShwYXJhbUluZGV4LCBkZWNvcmF0b3IpIHtcclxuICAgIHJldHVybiBmdW5jdGlvbiAodGFyZ2V0LCBrZXkpIHsgZGVjb3JhdG9yKHRhcmdldCwga2V5LCBwYXJhbUluZGV4KTsgfVxyXG59XHJcblxyXG5leHBvcnQgZnVuY3Rpb24gX19tZXRhZGF0YShtZXRhZGF0YUtleSwgbWV0YWRhdGFWYWx1ZSkge1xyXG4gICAgaWYgKHR5cGVvZiBSZWZsZWN0ID09PSBcIm9iamVjdFwiICYmIHR5cGVvZiBSZWZsZWN0Lm1ldGFkYXRhID09PSBcImZ1bmN0aW9uXCIpIHJldHVybiBSZWZsZWN0Lm1ldGFkYXRhKG1ldGFkYXRhS2V5LCBtZXRhZGF0YVZhbHVlKTtcclxufVxyXG5cclxuZXhwb3J0IGZ1bmN0aW9uIF9fYXdhaXRlcih0aGlzQXJnLCBfYXJndW1lbnRzLCBQLCBnZW5lcmF0b3IpIHtcclxuICAgIGZ1bmN0aW9uIGFkb3B0KHZhbHVlKSB7IHJldHVybiB2YWx1ZSBpbnN0YW5jZW9mIFAgPyB2YWx1ZSA6IG5ldyBQKGZ1bmN0aW9uIChyZXNvbHZlKSB7IHJlc29sdmUodmFsdWUpOyB9KTsgfVxyXG4gICAgcmV0dXJuIG5ldyAoUCB8fCAoUCA9IFByb21pc2UpKShmdW5jdGlvbiAocmVzb2x2ZSwgcmVqZWN0KSB7XHJcbiAgICAgICAgZnVuY3Rpb24gZnVsZmlsbGVkKHZhbHVlKSB7IHRyeSB7IHN0ZXAoZ2VuZXJhdG9yLm5leHQodmFsdWUpKTsgfSBjYXRjaCAoZSkgeyByZWplY3QoZSk7IH0gfVxyXG4gICAgICAgIGZ1bmN0aW9uIHJlamVjdGVkKHZhbHVlKSB7IHRyeSB7IHN0ZXAoZ2VuZXJhdG9yW1widGhyb3dcIl0odmFsdWUpKTsgfSBjYXRjaCAoZSkgeyByZWplY3QoZSk7IH0gfVxyXG4gICAgICAgIGZ1bmN0aW9uIHN0ZXAocmVzdWx0KSB7IHJlc3VsdC5kb25lID8gcmVzb2x2ZShyZXN1bHQudmFsdWUpIDogYWRvcHQocmVzdWx0LnZhbHVlKS50aGVuKGZ1bGZpbGxlZCwgcmVqZWN0ZWQpOyB9XHJcbiAgICAgICAgc3RlcCgoZ2VuZXJhdG9yID0gZ2VuZXJhdG9yLmFwcGx5KHRoaXNBcmcsIF9hcmd1bWVudHMgfHwgW10pKS5uZXh0KCkpO1xyXG4gICAgfSk7XHJcbn1cclxuXHJcbmV4cG9ydCBmdW5jdGlvbiBfX2dlbmVyYXRvcih0aGlzQXJnLCBib2R5KSB7XHJcbiAgICB2YXIgXyA9IHsgbGFiZWw6IDAsIHNlbnQ6IGZ1bmN0aW9uKCkgeyBpZiAodFswXSAmIDEpIHRocm93IHRbMV07IHJldHVybiB0WzFdOyB9LCB0cnlzOiBbXSwgb3BzOiBbXSB9LCBmLCB5LCB0LCBnO1xyXG4gICAgcmV0dXJuIGcgPSB7IG5leHQ6IHZlcmIoMCksIFwidGhyb3dcIjogdmVyYigxKSwgXCJyZXR1cm5cIjogdmVyYigyKSB9LCB0eXBlb2YgU3ltYm9sID09PSBcImZ1bmN0aW9uXCIgJiYgKGdbU3ltYm9sLml0ZXJhdG9yXSA9IGZ1bmN0aW9uKCkgeyByZXR1cm4gdGhpczsgfSksIGc7XHJcbiAgICBmdW5jdGlvbiB2ZXJiKG4pIHsgcmV0dXJuIGZ1bmN0aW9uICh2KSB7IHJldHVybiBzdGVwKFtuLCB2XSk7IH07IH1cclxuICAgIGZ1bmN0aW9uIHN0ZXAob3ApIHtcclxuICAgICAgICBpZiAoZikgdGhyb3cgbmV3IFR5cGVFcnJvcihcIkdlbmVyYXRvciBpcyBhbHJlYWR5IGV4ZWN1dGluZy5cIik7XHJcbiAgICAgICAgd2hpbGUgKF8pIHRyeSB7XHJcbiAgICAgICAgICAgIGlmIChmID0gMSwgeSAmJiAodCA9IG9wWzBdICYgMiA/IHlbXCJyZXR1cm5cIl0gOiBvcFswXSA/IHlbXCJ0aHJvd1wiXSB8fCAoKHQgPSB5W1wicmV0dXJuXCJdKSAmJiB0LmNhbGwoeSksIDApIDogeS5uZXh0KSAmJiAhKHQgPSB0LmNhbGwoeSwgb3BbMV0pKS5kb25lKSByZXR1cm4gdDtcclxuICAgICAgICAgICAgaWYgKHkgPSAwLCB0KSBvcCA9IFtvcFswXSAmIDIsIHQudmFsdWVdO1xyXG4gICAgICAgICAgICBzd2l0Y2ggKG9wWzBdKSB7XHJcbiAgICAgICAgICAgICAgICBjYXNlIDA6IGNhc2UgMTogdCA9IG9wOyBicmVhaztcclxuICAgICAgICAgICAgICAgIGNhc2UgNDogXy5sYWJlbCsrOyByZXR1cm4geyB2YWx1ZTogb3BbMV0sIGRvbmU6IGZhbHNlIH07XHJcbiAgICAgICAgICAgICAgICBjYXNlIDU6IF8ubGFiZWwrKzsgeSA9IG9wWzFdOyBvcCA9IFswXTsgY29udGludWU7XHJcbiAgICAgICAgICAgICAgICBjYXNlIDc6IG9wID0gXy5vcHMucG9wKCk7IF8udHJ5cy5wb3AoKTsgY29udGludWU7XHJcbiAgICAgICAgICAgICAgICBkZWZhdWx0OlxyXG4gICAgICAgICAgICAgICAgICAgIGlmICghKHQgPSBfLnRyeXMsIHQgPSB0Lmxlbmd0aCA+IDAgJiYgdFt0Lmxlbmd0aCAtIDFdKSAmJiAob3BbMF0gPT09IDYgfHwgb3BbMF0gPT09IDIpKSB7IF8gPSAwOyBjb250aW51ZTsgfVxyXG4gICAgICAgICAgICAgICAgICAgIGlmIChvcFswXSA9PT0gMyAmJiAoIXQgfHwgKG9wWzFdID4gdFswXSAmJiBvcFsxXSA8IHRbM10pKSkgeyBfLmxhYmVsID0gb3BbMV07IGJyZWFrOyB9XHJcbiAgICAgICAgICAgICAgICAgICAgaWYgKG9wWzBdID09PSA2ICYmIF8ubGFiZWwgPCB0WzFdKSB7IF8ubGFiZWwgPSB0WzFdOyB0ID0gb3A7IGJyZWFrOyB9XHJcbiAgICAgICAgICAgICAgICAgICAgaWYgKHQgJiYgXy5sYWJlbCA8IHRbMl0pIHsgXy5sYWJlbCA9IHRbMl07IF8ub3BzLnB1c2gob3ApOyBicmVhazsgfVxyXG4gICAgICAgICAgICAgICAgICAgIGlmICh0WzJdKSBfLm9wcy5wb3AoKTtcclxuICAgICAgICAgICAgICAgICAgICBfLnRyeXMucG9wKCk7IGNvbnRpbnVlO1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgICAgIG9wID0gYm9keS5jYWxsKHRoaXNBcmcsIF8pO1xyXG4gICAgICAgIH0gY2F0Y2ggKGUpIHsgb3AgPSBbNiwgZV07IHkgPSAwOyB9IGZpbmFsbHkgeyBmID0gdCA9IDA7IH1cclxuICAgICAgICBpZiAob3BbMF0gJiA1KSB0aHJvdyBvcFsxXTsgcmV0dXJuIHsgdmFsdWU6IG9wWzBdID8gb3BbMV0gOiB2b2lkIDAsIGRvbmU6IHRydWUgfTtcclxuICAgIH1cclxufVxyXG5cclxuZXhwb3J0IHZhciBfX2NyZWF0ZUJpbmRpbmcgPSBPYmplY3QuY3JlYXRlID8gKGZ1bmN0aW9uKG8sIG0sIGssIGsyKSB7XHJcbiAgICBpZiAoazIgPT09IHVuZGVmaW5lZCkgazIgPSBrO1xyXG4gICAgT2JqZWN0LmRlZmluZVByb3BlcnR5KG8sIGsyLCB7IGVudW1lcmFibGU6IHRydWUsIGdldDogZnVuY3Rpb24oKSB7IHJldHVybiBtW2tdOyB9IH0pO1xyXG59KSA6IChmdW5jdGlvbihvLCBtLCBrLCBrMikge1xyXG4gICAgaWYgKGsyID09PSB1bmRlZmluZWQpIGsyID0gaztcclxuICAgIG9bazJdID0gbVtrXTtcclxufSk7XHJcblxyXG5leHBvcnQgZnVuY3Rpb24gX19leHBvcnRTdGFyKG0sIG8pIHtcclxuICAgIGZvciAodmFyIHAgaW4gbSkgaWYgKHAgIT09IFwiZGVmYXVsdFwiICYmICFPYmplY3QucHJvdG90eXBlLmhhc093blByb3BlcnR5LmNhbGwobywgcCkpIF9fY3JlYXRlQmluZGluZyhvLCBtLCBwKTtcclxufVxyXG5cclxuZXhwb3J0IGZ1bmN0aW9uIF9fdmFsdWVzKG8pIHtcclxuICAgIHZhciBzID0gdHlwZW9mIFN5bWJvbCA9PT0gXCJmdW5jdGlvblwiICYmIFN5bWJvbC5pdGVyYXRvciwgbSA9IHMgJiYgb1tzXSwgaSA9IDA7XHJcbiAgICBpZiAobSkgcmV0dXJuIG0uY2FsbChvKTtcclxuICAgIGlmIChvICYmIHR5cGVvZiBvLmxlbmd0aCA9PT0gXCJudW1iZXJcIikgcmV0dXJuIHtcclxuICAgICAgICBuZXh0OiBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIGlmIChvICYmIGkgPj0gby5sZW5ndGgpIG8gPSB2b2lkIDA7XHJcbiAgICAgICAgICAgIHJldHVybiB7IHZhbHVlOiBvICYmIG9baSsrXSwgZG9uZTogIW8gfTtcclxuICAgICAgICB9XHJcbiAgICB9O1xyXG4gICAgdGhyb3cgbmV3IFR5cGVFcnJvcihzID8gXCJPYmplY3QgaXMgbm90IGl0ZXJhYmxlLlwiIDogXCJTeW1ib2wuaXRlcmF0b3IgaXMgbm90IGRlZmluZWQuXCIpO1xyXG59XHJcblxyXG5leHBvcnQgZnVuY3Rpb24gX19yZWFkKG8sIG4pIHtcclxuICAgIHZhciBtID0gdHlwZW9mIFN5bWJvbCA9PT0gXCJmdW5jdGlvblwiICYmIG9bU3ltYm9sLml0ZXJhdG9yXTtcclxuICAgIGlmICghbSkgcmV0dXJuIG87XHJcbiAgICB2YXIgaSA9IG0uY2FsbChvKSwgciwgYXIgPSBbXSwgZTtcclxuICAgIHRyeSB7XHJcbiAgICAgICAgd2hpbGUgKChuID09PSB2b2lkIDAgfHwgbi0tID4gMCkgJiYgIShyID0gaS5uZXh0KCkpLmRvbmUpIGFyLnB1c2goci52YWx1ZSk7XHJcbiAgICB9XHJcbiAgICBjYXRjaCAoZXJyb3IpIHsgZSA9IHsgZXJyb3I6IGVycm9yIH07IH1cclxuICAgIGZpbmFsbHkge1xyXG4gICAgICAgIHRyeSB7XHJcbiAgICAgICAgICAgIGlmIChyICYmICFyLmRvbmUgJiYgKG0gPSBpW1wicmV0dXJuXCJdKSkgbS5jYWxsKGkpO1xyXG4gICAgICAgIH1cclxuICAgICAgICBmaW5hbGx5IHsgaWYgKGUpIHRocm93IGUuZXJyb3I7IH1cclxuICAgIH1cclxuICAgIHJldHVybiBhcjtcclxufVxyXG5cclxuZXhwb3J0IGZ1bmN0aW9uIF9fc3ByZWFkKCkge1xyXG4gICAgZm9yICh2YXIgYXIgPSBbXSwgaSA9IDA7IGkgPCBhcmd1bWVudHMubGVuZ3RoOyBpKyspXHJcbiAgICAgICAgYXIgPSBhci5jb25jYXQoX19yZWFkKGFyZ3VtZW50c1tpXSkpO1xyXG4gICAgcmV0dXJuIGFyO1xyXG59XHJcblxyXG5leHBvcnQgZnVuY3Rpb24gX19zcHJlYWRBcnJheXMoKSB7XHJcbiAgICBmb3IgKHZhciBzID0gMCwgaSA9IDAsIGlsID0gYXJndW1lbnRzLmxlbmd0aDsgaSA8IGlsOyBpKyspIHMgKz0gYXJndW1lbnRzW2ldLmxlbmd0aDtcclxuICAgIGZvciAodmFyIHIgPSBBcnJheShzKSwgayA9IDAsIGkgPSAwOyBpIDwgaWw7IGkrKylcclxuICAgICAgICBmb3IgKHZhciBhID0gYXJndW1lbnRzW2ldLCBqID0gMCwgamwgPSBhLmxlbmd0aDsgaiA8IGpsOyBqKyssIGsrKylcclxuICAgICAgICAgICAgcltrXSA9IGFbal07XHJcbiAgICByZXR1cm4gcjtcclxufTtcclxuXHJcbmV4cG9ydCBmdW5jdGlvbiBfX2F3YWl0KHYpIHtcclxuICAgIHJldHVybiB0aGlzIGluc3RhbmNlb2YgX19hd2FpdCA/ICh0aGlzLnYgPSB2LCB0aGlzKSA6IG5ldyBfX2F3YWl0KHYpO1xyXG59XHJcblxyXG5leHBvcnQgZnVuY3Rpb24gX19hc3luY0dlbmVyYXRvcih0aGlzQXJnLCBfYXJndW1lbnRzLCBnZW5lcmF0b3IpIHtcclxuICAgIGlmICghU3ltYm9sLmFzeW5jSXRlcmF0b3IpIHRocm93IG5ldyBUeXBlRXJyb3IoXCJTeW1ib2wuYXN5bmNJdGVyYXRvciBpcyBub3QgZGVmaW5lZC5cIik7XHJcbiAgICB2YXIgZyA9IGdlbmVyYXRvci5hcHBseSh0aGlzQXJnLCBfYXJndW1lbnRzIHx8IFtdKSwgaSwgcSA9IFtdO1xyXG4gICAgcmV0dXJuIGkgPSB7fSwgdmVyYihcIm5leHRcIiksIHZlcmIoXCJ0aHJvd1wiKSwgdmVyYihcInJldHVyblwiKSwgaVtTeW1ib2wuYXN5bmNJdGVyYXRvcl0gPSBmdW5jdGlvbiAoKSB7IHJldHVybiB0aGlzOyB9LCBpO1xyXG4gICAgZnVuY3Rpb24gdmVyYihuKSB7IGlmIChnW25dKSBpW25dID0gZnVuY3Rpb24gKHYpIHsgcmV0dXJuIG5ldyBQcm9taXNlKGZ1bmN0aW9uIChhLCBiKSB7IHEucHVzaChbbiwgdiwgYSwgYl0pID4gMSB8fCByZXN1bWUobiwgdik7IH0pOyB9OyB9XHJcbiAgICBmdW5jdGlvbiByZXN1bWUobiwgdikgeyB0cnkgeyBzdGVwKGdbbl0odikpOyB9IGNhdGNoIChlKSB7IHNldHRsZShxWzBdWzNdLCBlKTsgfSB9XHJcbiAgICBmdW5jdGlvbiBzdGVwKHIpIHsgci52YWx1ZSBpbnN0YW5jZW9mIF9fYXdhaXQgPyBQcm9taXNlLnJlc29sdmUoci52YWx1ZS52KS50aGVuKGZ1bGZpbGwsIHJlamVjdCkgOiBzZXR0bGUocVswXVsyXSwgcik7IH1cclxuICAgIGZ1bmN0aW9uIGZ1bGZpbGwodmFsdWUpIHsgcmVzdW1lKFwibmV4dFwiLCB2YWx1ZSk7IH1cclxuICAgIGZ1bmN0aW9uIHJlamVjdCh2YWx1ZSkgeyByZXN1bWUoXCJ0aHJvd1wiLCB2YWx1ZSk7IH1cclxuICAgIGZ1bmN0aW9uIHNldHRsZShmLCB2KSB7IGlmIChmKHYpLCBxLnNoaWZ0KCksIHEubGVuZ3RoKSByZXN1bWUocVswXVswXSwgcVswXVsxXSk7IH1cclxufVxyXG5cclxuZXhwb3J0IGZ1bmN0aW9uIF9fYXN5bmNEZWxlZ2F0b3Iobykge1xyXG4gICAgdmFyIGksIHA7XHJcbiAgICByZXR1cm4gaSA9IHt9LCB2ZXJiKFwibmV4dFwiKSwgdmVyYihcInRocm93XCIsIGZ1bmN0aW9uIChlKSB7IHRocm93IGU7IH0pLCB2ZXJiKFwicmV0dXJuXCIpLCBpW1N5bWJvbC5pdGVyYXRvcl0gPSBmdW5jdGlvbiAoKSB7IHJldHVybiB0aGlzOyB9LCBpO1xyXG4gICAgZnVuY3Rpb24gdmVyYihuLCBmKSB7IGlbbl0gPSBvW25dID8gZnVuY3Rpb24gKHYpIHsgcmV0dXJuIChwID0gIXApID8geyB2YWx1ZTogX19hd2FpdChvW25dKHYpKSwgZG9uZTogbiA9PT0gXCJyZXR1cm5cIiB9IDogZiA/IGYodikgOiB2OyB9IDogZjsgfVxyXG59XHJcblxyXG5leHBvcnQgZnVuY3Rpb24gX19hc3luY1ZhbHVlcyhvKSB7XHJcbiAgICBpZiAoIVN5bWJvbC5hc3luY0l0ZXJhdG9yKSB0aHJvdyBuZXcgVHlwZUVycm9yKFwiU3ltYm9sLmFzeW5jSXRlcmF0b3IgaXMgbm90IGRlZmluZWQuXCIpO1xyXG4gICAgdmFyIG0gPSBvW1N5bWJvbC5hc3luY0l0ZXJhdG9yXSwgaTtcclxuICAgIHJldHVybiBtID8gbS5jYWxsKG8pIDogKG8gPSB0eXBlb2YgX192YWx1ZXMgPT09IFwiZnVuY3Rpb25cIiA/IF9fdmFsdWVzKG8pIDogb1tTeW1ib2wuaXRlcmF0b3JdKCksIGkgPSB7fSwgdmVyYihcIm5leHRcIiksIHZlcmIoXCJ0aHJvd1wiKSwgdmVyYihcInJldHVyblwiKSwgaVtTeW1ib2wuYXN5bmNJdGVyYXRvcl0gPSBmdW5jdGlvbiAoKSB7IHJldHVybiB0aGlzOyB9LCBpKTtcclxuICAgIGZ1bmN0aW9uIHZlcmIobikgeyBpW25dID0gb1tuXSAmJiBmdW5jdGlvbiAodikgeyByZXR1cm4gbmV3IFByb21pc2UoZnVuY3Rpb24gKHJlc29sdmUsIHJlamVjdCkgeyB2ID0gb1tuXSh2KSwgc2V0dGxlKHJlc29sdmUsIHJlamVjdCwgdi5kb25lLCB2LnZhbHVlKTsgfSk7IH07IH1cclxuICAgIGZ1bmN0aW9uIHNldHRsZShyZXNvbHZlLCByZWplY3QsIGQsIHYpIHsgUHJvbWlzZS5yZXNvbHZlKHYpLnRoZW4oZnVuY3Rpb24odikgeyByZXNvbHZlKHsgdmFsdWU6IHYsIGRvbmU6IGQgfSk7IH0sIHJlamVjdCk7IH1cclxufVxyXG5cclxuZXhwb3J0IGZ1bmN0aW9uIF9fbWFrZVRlbXBsYXRlT2JqZWN0KGNvb2tlZCwgcmF3KSB7XHJcbiAgICBpZiAoT2JqZWN0LmRlZmluZVByb3BlcnR5KSB7IE9iamVjdC5kZWZpbmVQcm9wZXJ0eShjb29rZWQsIFwicmF3XCIsIHsgdmFsdWU6IHJhdyB9KTsgfSBlbHNlIHsgY29va2VkLnJhdyA9IHJhdzsgfVxyXG4gICAgcmV0dXJuIGNvb2tlZDtcclxufTtcclxuXHJcbnZhciBfX3NldE1vZHVsZURlZmF1bHQgPSBPYmplY3QuY3JlYXRlID8gKGZ1bmN0aW9uKG8sIHYpIHtcclxuICAgIE9iamVjdC5kZWZpbmVQcm9wZXJ0eShvLCBcImRlZmF1bHRcIiwgeyBlbnVtZXJhYmxlOiB0cnVlLCB2YWx1ZTogdiB9KTtcclxufSkgOiBmdW5jdGlvbihvLCB2KSB7XHJcbiAgICBvW1wiZGVmYXVsdFwiXSA9IHY7XHJcbn07XHJcblxyXG5leHBvcnQgZnVuY3Rpb24gX19pbXBvcnRTdGFyKG1vZCkge1xyXG4gICAgaWYgKG1vZCAmJiBtb2QuX19lc01vZHVsZSkgcmV0dXJuIG1vZDtcclxuICAgIHZhciByZXN1bHQgPSB7fTtcclxuICAgIGlmIChtb2QgIT0gbnVsbCkgZm9yICh2YXIgayBpbiBtb2QpIGlmIChrICE9PSBcImRlZmF1bHRcIiAmJiBPYmplY3QucHJvdG90eXBlLmhhc093blByb3BlcnR5LmNhbGwobW9kLCBrKSkgX19jcmVhdGVCaW5kaW5nKHJlc3VsdCwgbW9kLCBrKTtcclxuICAgIF9fc2V0TW9kdWxlRGVmYXVsdChyZXN1bHQsIG1vZCk7XHJcbiAgICByZXR1cm4gcmVzdWx0O1xyXG59XHJcblxyXG5leHBvcnQgZnVuY3Rpb24gX19pbXBvcnREZWZhdWx0KG1vZCkge1xyXG4gICAgcmV0dXJuIChtb2QgJiYgbW9kLl9fZXNNb2R1bGUpID8gbW9kIDogeyBkZWZhdWx0OiBtb2QgfTtcclxufVxyXG5cclxuZXhwb3J0IGZ1bmN0aW9uIF9fY2xhc3NQcml2YXRlRmllbGRHZXQocmVjZWl2ZXIsIHByaXZhdGVNYXApIHtcclxuICAgIGlmICghcHJpdmF0ZU1hcC5oYXMocmVjZWl2ZXIpKSB7XHJcbiAgICAgICAgdGhyb3cgbmV3IFR5cGVFcnJvcihcImF0dGVtcHRlZCB0byBnZXQgcHJpdmF0ZSBmaWVsZCBvbiBub24taW5zdGFuY2VcIik7XHJcbiAgICB9XHJcbiAgICByZXR1cm4gcHJpdmF0ZU1hcC5nZXQocmVjZWl2ZXIpO1xyXG59XHJcblxyXG5leHBvcnQgZnVuY3Rpb24gX19jbGFzc1ByaXZhdGVGaWVsZFNldChyZWNlaXZlciwgcHJpdmF0ZU1hcCwgdmFsdWUpIHtcclxuICAgIGlmICghcHJpdmF0ZU1hcC5oYXMocmVjZWl2ZXIpKSB7XHJcbiAgICAgICAgdGhyb3cgbmV3IFR5cGVFcnJvcihcImF0dGVtcHRlZCB0byBzZXQgcHJpdmF0ZSBmaWVsZCBvbiBub24taW5zdGFuY2VcIik7XHJcbiAgICB9XHJcbiAgICBwcml2YXRlTWFwLnNldChyZWNlaXZlciwgdmFsdWUpO1xyXG4gICAgcmV0dXJuIHZhbHVlO1xyXG59XHJcbiIsIid1c2Ugc3RyaWN0JztcblxubGV0IHJlZ2lzdGVyO1xudHJ5IHtcbiAgY29uc3QgeyBhcHAgfSA9IHJlcXVpcmUoJ2VsZWN0cm9uJyk7XG4gIHJlZ2lzdGVyID0gYXBwLnNldEFzRGVmYXVsdFByb3RvY29sQ2xpZW50LmJpbmQoYXBwKTtcbn0gY2F0Y2ggKGVycikge1xuICB0cnkge1xuICAgIHJlZ2lzdGVyID0gcmVxdWlyZSgncmVnaXN0ZXItc2NoZW1lJyk7XG4gIH0gY2F0Y2ggKGUpIHt9IC8vIGVzbGludC1kaXNhYmxlLWxpbmUgbm8tZW1wdHlcbn1cblxuaWYgKHR5cGVvZiByZWdpc3RlciAhPT0gJ2Z1bmN0aW9uJykge1xuICByZWdpc3RlciA9ICgpID0+IGZhbHNlO1xufVxuXG5mdW5jdGlvbiBwaWQoKSB7XG4gIGlmICh0eXBlb2YgcHJvY2VzcyAhPT0gJ3VuZGVmaW5lZCcpIHtcbiAgICByZXR1cm4gcHJvY2Vzcy5waWQ7XG4gIH1cbiAgcmV0dXJuIG51bGw7XG59XG5cbmNvbnN0IHV1aWQ0MTIyID0gKCkgPT4ge1xuICBsZXQgdXVpZCA9ICcnO1xuICBmb3IgKGxldCBpID0gMDsgaSA8IDMyOyBpICs9IDEpIHtcbiAgICBpZiAoaSA9PT0gOCB8fCBpID09PSAxMiB8fCBpID09PSAxNiB8fCBpID09PSAyMCkge1xuICAgICAgdXVpZCArPSAnLSc7XG4gICAgfVxuICAgIGxldCBuO1xuICAgIGlmIChpID09PSAxMikge1xuICAgICAgbiA9IDQ7XG4gICAgfSBlbHNlIHtcbiAgICAgIGNvbnN0IHJhbmRvbSA9IE1hdGgucmFuZG9tKCkgKiAxNiB8IDA7XG4gICAgICBpZiAoaSA9PT0gMTYpIHtcbiAgICAgICAgbiA9IChyYW5kb20gJiAzKSB8IDA7XG4gICAgICB9IGVsc2Uge1xuICAgICAgICBuID0gcmFuZG9tO1xuICAgICAgfVxuICAgIH1cbiAgICB1dWlkICs9IG4udG9TdHJpbmcoMTYpO1xuICB9XG4gIHJldHVybiB1dWlkO1xufTtcblxubW9kdWxlLmV4cG9ydHMgPSB7XG4gIHBpZCxcbiAgcmVnaXN0ZXIsXG4gIHV1aWQ6IHV1aWQ0MTIyLFxufTtcbiIsIlwidXNlIHN0cmljdFwiO1xuXG4vLyByZWY6IGh0dHBzOi8vZ2l0aHViLmNvbS90YzM5L3Byb3Bvc2FsLWdsb2JhbFxudmFyIGdldEdsb2JhbCA9IGZ1bmN0aW9uICgpIHtcblx0Ly8gdGhlIG9ubHkgcmVsaWFibGUgbWVhbnMgdG8gZ2V0IHRoZSBnbG9iYWwgb2JqZWN0IGlzXG5cdC8vIGBGdW5jdGlvbigncmV0dXJuIHRoaXMnKSgpYFxuXHQvLyBIb3dldmVyLCB0aGlzIGNhdXNlcyBDU1AgdmlvbGF0aW9ucyBpbiBDaHJvbWUgYXBwcy5cblx0aWYgKHR5cGVvZiBzZWxmICE9PSAndW5kZWZpbmVkJykgeyByZXR1cm4gc2VsZjsgfVxuXHRpZiAodHlwZW9mIHdpbmRvdyAhPT0gJ3VuZGVmaW5lZCcpIHsgcmV0dXJuIHdpbmRvdzsgfVxuXHRpZiAodHlwZW9mIGdsb2JhbCAhPT0gJ3VuZGVmaW5lZCcpIHsgcmV0dXJuIGdsb2JhbDsgfVxuXHR0aHJvdyBuZXcgRXJyb3IoJ3VuYWJsZSB0byBsb2NhdGUgZ2xvYmFsIG9iamVjdCcpO1xufVxuXG52YXIgZ2xvYmFsID0gZ2V0R2xvYmFsKCk7XG5cbm1vZHVsZS5leHBvcnRzID0gZXhwb3J0cyA9IGdsb2JhbC5mZXRjaDtcblxuLy8gTmVlZGVkIGZvciBUeXBlU2NyaXB0IGFuZCBXZWJwYWNrLlxuaWYgKGdsb2JhbC5mZXRjaCkge1xuXHRleHBvcnRzLmRlZmF1bHQgPSBnbG9iYWwuZmV0Y2guYmluZChnbG9iYWwpO1xufVxuXG5leHBvcnRzLkhlYWRlcnMgPSBnbG9iYWwuSGVhZGVycztcbmV4cG9ydHMuUmVxdWVzdCA9IGdsb2JhbC5SZXF1ZXN0O1xuZXhwb3J0cy5SZXNwb25zZSA9IGdsb2JhbC5SZXNwb25zZTsiLCIndXNlIHN0cmljdCc7XG5cbmNvbnN0IG5ldCA9IHJlcXVpcmUoJ25ldCcpO1xuY29uc3QgRXZlbnRFbWl0dGVyID0gcmVxdWlyZSgnZXZlbnRzJyk7XG5jb25zdCBmZXRjaCA9IHJlcXVpcmUoJ25vZGUtZmV0Y2gnKTtcbmNvbnN0IHsgdXVpZCB9ID0gcmVxdWlyZSgnLi4vdXRpbCcpO1xuXG5jb25zdCBPUENvZGVzID0ge1xuICBIQU5EU0hBS0U6IDAsXG4gIEZSQU1FOiAxLFxuICBDTE9TRTogMixcbiAgUElORzogMyxcbiAgUE9ORzogNCxcbn07XG5cbmZ1bmN0aW9uIGdldElQQ1BhdGgoaWQpIHtcbiAgaWYgKHByb2Nlc3MucGxhdGZvcm0gPT09ICd3aW4zMicpIHtcbiAgICByZXR1cm4gYFxcXFxcXFxcP1xcXFxwaXBlXFxcXGRpc2NvcmQtaXBjLSR7aWR9YDtcbiAgfVxuICBjb25zdCB7IGVudjogeyBYREdfUlVOVElNRV9ESVIsIFRNUERJUiwgVE1QLCBURU1QIH0gfSA9IHByb2Nlc3M7XG4gIGNvbnN0IHByZWZpeCA9IFhER19SVU5USU1FX0RJUiB8fCBUTVBESVIgfHwgVE1QIHx8IFRFTVAgfHwgJy90bXAnO1xuICByZXR1cm4gYCR7cHJlZml4LnJlcGxhY2UoL1xcLyQvLCAnJyl9L2Rpc2NvcmQtaXBjLSR7aWR9YDtcbn1cblxuZnVuY3Rpb24gZ2V0SVBDKGlkID0gMCkge1xuICByZXR1cm4gbmV3IFByb21pc2UoKHJlc29sdmUsIHJlamVjdCkgPT4ge1xuICAgIGNvbnN0IHBhdGggPSBnZXRJUENQYXRoKGlkKTtcbiAgICBjb25zdCBvbmVycm9yID0gKCkgPT4ge1xuICAgICAgaWYgKGlkIDwgMTApIHtcbiAgICAgICAgcmVzb2x2ZShnZXRJUEMoaWQgKyAxKSk7XG4gICAgICB9IGVsc2Uge1xuICAgICAgICByZWplY3QobmV3IEVycm9yKCdDb3VsZCBub3QgY29ubmVjdCcpKTtcbiAgICAgIH1cbiAgICB9O1xuICAgIGNvbnN0IHNvY2sgPSBuZXQuY3JlYXRlQ29ubmVjdGlvbihwYXRoLCAoKSA9PiB7XG4gICAgICBzb2NrLnJlbW92ZUxpc3RlbmVyKCdlcnJvcicsIG9uZXJyb3IpO1xuICAgICAgcmVzb2x2ZShzb2NrKTtcbiAgICB9KTtcbiAgICBzb2NrLm9uY2UoJ2Vycm9yJywgb25lcnJvcik7XG4gIH0pO1xufVxuXG5hc3luYyBmdW5jdGlvbiBmaW5kRW5kcG9pbnQodHJpZXMgPSAwKSB7XG4gIGlmICh0cmllcyA+IDMwKSB7XG4gICAgdGhyb3cgbmV3IEVycm9yKCdDb3VsZCBub3QgZmluZCBlbmRwb2ludCcpO1xuICB9XG4gIGNvbnN0IGVuZHBvaW50ID0gYGh0dHA6Ly8xMjcuMC4wLjE6JHs2NDYzICsgKHRyaWVzICUgMTApfWA7XG4gIHRyeSB7XG4gICAgY29uc3QgciA9IGF3YWl0IGZldGNoKGVuZHBvaW50KTtcbiAgICBpZiAoci5zdGF0dXMgPT09IDQwNCkge1xuICAgICAgcmV0dXJuIGVuZHBvaW50O1xuICAgIH1cbiAgICByZXR1cm4gZmluZEVuZHBvaW50KHRyaWVzICsgMSk7XG4gIH0gY2F0Y2ggKGUpIHtcbiAgICByZXR1cm4gZmluZEVuZHBvaW50KHRyaWVzICsgMSk7XG4gIH1cbn1cblxuZnVuY3Rpb24gZW5jb2RlKG9wLCBkYXRhKSB7XG4gIGRhdGEgPSBKU09OLnN0cmluZ2lmeShkYXRhKTtcbiAgY29uc3QgbGVuID0gQnVmZmVyLmJ5dGVMZW5ndGgoZGF0YSk7XG4gIGNvbnN0IHBhY2tldCA9IEJ1ZmZlci5hbGxvYyg4ICsgbGVuKTtcbiAgcGFja2V0LndyaXRlSW50MzJMRShvcCwgMCk7XG4gIHBhY2tldC53cml0ZUludDMyTEUobGVuLCA0KTtcbiAgcGFja2V0LndyaXRlKGRhdGEsIDgsIGxlbik7XG4gIHJldHVybiBwYWNrZXQ7XG59XG5cbmNvbnN0IHdvcmtpbmcgPSB7XG4gIGZ1bGw6ICcnLFxuICBvcDogdW5kZWZpbmVkLFxufTtcblxuZnVuY3Rpb24gZGVjb2RlKHNvY2tldCwgY2FsbGJhY2spIHtcbiAgY29uc3QgcGFja2V0ID0gc29ja2V0LnJlYWQoKTtcbiAgaWYgKCFwYWNrZXQpIHtcbiAgICByZXR1cm47XG4gIH1cblxuICBsZXQgeyBvcCB9ID0gd29ya2luZztcbiAgbGV0IHJhdztcbiAgaWYgKHdvcmtpbmcuZnVsbCA9PT0gJycpIHtcbiAgICBvcCA9IHdvcmtpbmcub3AgPSBwYWNrZXQucmVhZEludDMyTEUoMCk7XG4gICAgY29uc3QgbGVuID0gcGFja2V0LnJlYWRJbnQzMkxFKDQpO1xuICAgIHJhdyA9IHBhY2tldC5zbGljZSg4LCBsZW4gKyA4KTtcbiAgfSBlbHNlIHtcbiAgICByYXcgPSBwYWNrZXQudG9TdHJpbmcoKTtcbiAgfVxuXG4gIHRyeSB7XG4gICAgY29uc3QgZGF0YSA9IEpTT04ucGFyc2Uod29ya2luZy5mdWxsICsgcmF3KTtcbiAgICBjYWxsYmFjayh7IG9wLCBkYXRhIH0pOyAvLyBlc2xpbnQtZGlzYWJsZS1saW5lIGNhbGxiYWNrLXJldHVyblxuICAgIHdvcmtpbmcuZnVsbCA9ICcnO1xuICAgIHdvcmtpbmcub3AgPSB1bmRlZmluZWQ7XG4gIH0gY2F0Y2ggKGVycikge1xuICAgIHdvcmtpbmcuZnVsbCArPSByYXc7XG4gIH1cblxuICBkZWNvZGUoc29ja2V0LCBjYWxsYmFjayk7XG59XG5cbmNsYXNzIElQQ1RyYW5zcG9ydCBleHRlbmRzIEV2ZW50RW1pdHRlciB7XG4gIGNvbnN0cnVjdG9yKGNsaWVudCkge1xuICAgIHN1cGVyKCk7XG4gICAgdGhpcy5jbGllbnQgPSBjbGllbnQ7XG4gICAgdGhpcy5zb2NrZXQgPSBudWxsO1xuICB9XG5cbiAgYXN5bmMgY29ubmVjdCgpIHtcbiAgICBjb25zdCBzb2NrZXQgPSB0aGlzLnNvY2tldCA9IGF3YWl0IGdldElQQygpO1xuICAgIHNvY2tldC5vbignY2xvc2UnLCB0aGlzLm9uQ2xvc2UuYmluZCh0aGlzKSk7XG4gICAgc29ja2V0Lm9uKCdlcnJvcicsIHRoaXMub25DbG9zZS5iaW5kKHRoaXMpKTtcbiAgICB0aGlzLmVtaXQoJ29wZW4nKTtcbiAgICBzb2NrZXQud3JpdGUoZW5jb2RlKE9QQ29kZXMuSEFORFNIQUtFLCB7XG4gICAgICB2OiAxLFxuICAgICAgY2xpZW50X2lkOiB0aGlzLmNsaWVudC5jbGllbnRJZCxcbiAgICB9KSk7XG4gICAgc29ja2V0LnBhdXNlKCk7XG4gICAgc29ja2V0Lm9uKCdyZWFkYWJsZScsICgpID0+IHtcbiAgICAgIGRlY29kZShzb2NrZXQsICh7IG9wLCBkYXRhIH0pID0+IHtcbiAgICAgICAgc3dpdGNoIChvcCkge1xuICAgICAgICAgIGNhc2UgT1BDb2Rlcy5QSU5HOlxuICAgICAgICAgICAgdGhpcy5zZW5kKGRhdGEsIE9QQ29kZXMuUE9ORyk7XG4gICAgICAgICAgICBicmVhaztcbiAgICAgICAgICBjYXNlIE9QQ29kZXMuRlJBTUU6XG4gICAgICAgICAgICBpZiAoIWRhdGEpIHtcbiAgICAgICAgICAgICAgcmV0dXJuO1xuICAgICAgICAgICAgfVxuICAgICAgICAgICAgaWYgKGRhdGEuY21kID09PSAnQVVUSE9SSVpFJyAmJiBkYXRhLmV2dCAhPT0gJ0VSUk9SJykge1xuICAgICAgICAgICAgICBmaW5kRW5kcG9pbnQoKS50aGVuKChlbmRwb2ludCkgPT4ge1xuICAgICAgICAgICAgICAgIHRoaXMuY2xpZW50LnJlcXVlc3QuZW5kcG9pbnQgPSBlbmRwb2ludDtcbiAgICAgICAgICAgICAgfSk7XG4gICAgICAgICAgICB9XG4gICAgICAgICAgICB0aGlzLmVtaXQoJ21lc3NhZ2UnLCBkYXRhKTtcbiAgICAgICAgICAgIGJyZWFrO1xuICAgICAgICAgIGNhc2UgT1BDb2Rlcy5DTE9TRTpcbiAgICAgICAgICAgIHRoaXMuZW1pdCgnY2xvc2UnLCBkYXRhKTtcbiAgICAgICAgICAgIGJyZWFrO1xuICAgICAgICAgIGRlZmF1bHQ6XG4gICAgICAgICAgICBicmVhaztcbiAgICAgICAgfVxuICAgICAgfSk7XG4gICAgfSk7XG4gIH1cblxuICBvbkNsb3NlKGUpIHtcbiAgICB0aGlzLmVtaXQoJ2Nsb3NlJywgZSk7XG4gIH1cblxuICBzZW5kKGRhdGEsIG9wID0gT1BDb2Rlcy5GUkFNRSkge1xuICAgIHRoaXMuc29ja2V0LndyaXRlKGVuY29kZShvcCwgZGF0YSkpO1xuICB9XG5cbiAgY2xvc2UoKSB7XG4gICAgdGhpcy5zZW5kKHt9LCBPUENvZGVzLkNMT1NFKTtcbiAgICB0aGlzLnNvY2tldC5lbmQoKTtcbiAgfVxuXG4gIHBpbmcoKSB7XG4gICAgdGhpcy5zZW5kKHV1aWQoKSwgT1BDb2Rlcy5QSU5HKTtcbiAgfVxufVxuXG5tb2R1bGUuZXhwb3J0cyA9IElQQ1RyYW5zcG9ydDtcbm1vZHVsZS5leHBvcnRzLmVuY29kZSA9IGVuY29kZTtcbm1vZHVsZS5leHBvcnRzLmRlY29kZSA9IGRlY29kZTtcbiIsIid1c2Ugc3RyaWN0JztcblxuZnVuY3Rpb24ga2V5TWlycm9yKGFycikge1xuICBjb25zdCB0bXAgPSB7fTtcbiAgZm9yIChjb25zdCB2YWx1ZSBvZiBhcnIpIHtcbiAgICB0bXBbdmFsdWVdID0gdmFsdWU7XG4gIH1cbiAgcmV0dXJuIHRtcDtcbn1cblxuXG5leHBvcnRzLmJyb3dzZXIgPSB0eXBlb2Ygd2luZG93ICE9PSAndW5kZWZpbmVkJztcblxuZXhwb3J0cy5SUENDb21tYW5kcyA9IGtleU1pcnJvcihbXG4gICdESVNQQVRDSCcsXG4gICdBVVRIT1JJWkUnLFxuICAnQVVUSEVOVElDQVRFJyxcbiAgJ0dFVF9HVUlMRCcsXG4gICdHRVRfR1VJTERTJyxcbiAgJ0dFVF9DSEFOTkVMJyxcbiAgJ0dFVF9DSEFOTkVMUycsXG4gICdDUkVBVEVfQ0hBTk5FTF9JTlZJVEUnLFxuICAnR0VUX1JFTEFUSU9OU0hJUFMnLFxuICAnR0VUX1VTRVInLFxuICAnU1VCU0NSSUJFJyxcbiAgJ1VOU1VCU0NSSUJFJyxcbiAgJ1NFVF9VU0VSX1ZPSUNFX1NFVFRJTkdTJyxcbiAgJ1NFVF9VU0VSX1ZPSUNFX1NFVFRJTkdTXzInLFxuICAnU0VMRUNUX1ZPSUNFX0NIQU5ORUwnLFxuICAnR0VUX1NFTEVDVEVEX1ZPSUNFX0NIQU5ORUwnLFxuICAnU0VMRUNUX1RFWFRfQ0hBTk5FTCcsXG4gICdHRVRfVk9JQ0VfU0VUVElOR1MnLFxuICAnU0VUX1ZPSUNFX1NFVFRJTkdTXzInLFxuICAnU0VUX1ZPSUNFX1NFVFRJTkdTJyxcbiAgJ0NBUFRVUkVfU0hPUlRDVVQnLFxuICAnU0VUX0FDVElWSVRZJyxcbiAgJ1NFTkRfQUNUSVZJVFlfSk9JTl9JTlZJVEUnLFxuICAnQ0xPU0VfQUNUSVZJVFlfSk9JTl9SRVFVRVNUJyxcbiAgJ0FDVElWSVRZX0lOVklURV9VU0VSJyxcbiAgJ0FDQ0VQVF9BQ1RJVklUWV9JTlZJVEUnLFxuICAnSU5WSVRFX0JST1dTRVInLFxuICAnREVFUF9MSU5LJyxcbiAgJ0NPTk5FQ1RJT05TX0NBTExCQUNLJyxcbiAgJ0JSQUlOVFJFRV9QT1BVUF9CUklER0VfQ0FMTEJBQ0snLFxuICAnR0lGVF9DT0RFX0JST1dTRVInLFxuICAnR1VJTERfVEVNUExBVEVfQlJPV1NFUicsXG4gICdPVkVSTEFZJyxcbiAgJ0JST1dTRVJfSEFORE9GRicsXG4gICdTRVRfQ0VSVElGSUVEX0RFVklDRVMnLFxuICAnR0VUX0lNQUdFJyxcbiAgJ0NSRUFURV9MT0JCWScsXG4gICdVUERBVEVfTE9CQlknLFxuICAnREVMRVRFX0xPQkJZJyxcbiAgJ1VQREFURV9MT0JCWV9NRU1CRVInLFxuICAnQ09OTkVDVF9UT19MT0JCWScsXG4gICdESVNDT05ORUNUX0ZST01fTE9CQlknLFxuICAnU0VORF9UT19MT0JCWScsXG4gICdTRUFSQ0hfTE9CQklFUycsXG4gICdDT05ORUNUX1RPX0xPQkJZX1ZPSUNFJyxcbiAgJ0RJU0NPTk5FQ1RfRlJPTV9MT0JCWV9WT0lDRScsXG4gICdTRVRfT1ZFUkxBWV9MT0NLRUQnLFxuICAnT1BFTl9PVkVSTEFZX0FDVElWSVRZX0lOVklURScsXG4gICdPUEVOX09WRVJMQVlfR1VJTERfSU5WSVRFJyxcbiAgJ09QRU5fT1ZFUkxBWV9WT0lDRV9TRVRUSU5HUycsXG4gICdWQUxJREFURV9BUFBMSUNBVElPTicsXG4gICdHRVRfRU5USVRMRU1FTlRfVElDS0VUJyxcbiAgJ0dFVF9BUFBMSUNBVElPTl9USUNLRVQnLFxuICAnU1RBUlRfUFVSQ0hBU0UnLFxuICAnR0VUX1NLVVMnLFxuICAnR0VUX0VOVElUTEVNRU5UUycsXG4gICdHRVRfTkVUV09SS0lOR19DT05GSUcnLFxuICAnTkVUV09SS0lOR19TWVNURU1fTUVUUklDUycsXG4gICdORVRXT1JLSU5HX1BFRVJfTUVUUklDUycsXG4gICdORVRXT1JLSU5HX0NSRUFURV9UT0tFTicsXG4gICdTRVRfVVNFUl9BQ0hJRVZFTUVOVCcsXG4gICdHRVRfVVNFUl9BQ0hJRVZFTUVOVFMnLFxuXSk7XG5cbmV4cG9ydHMuUlBDRXZlbnRzID0ga2V5TWlycm9yKFtcbiAgJ0NVUlJFTlRfVVNFUl9VUERBVEUnLFxuICAnR1VJTERfU1RBVFVTJyxcbiAgJ0dVSUxEX0NSRUFURScsXG4gICdDSEFOTkVMX0NSRUFURScsXG4gICdSRUxBVElPTlNISVBfVVBEQVRFJyxcbiAgJ1ZPSUNFX0NIQU5ORUxfU0VMRUNUJyxcbiAgJ1ZPSUNFX1NUQVRFX0NSRUFURScsXG4gICdWT0lDRV9TVEFURV9ERUxFVEUnLFxuICAnVk9JQ0VfU1RBVEVfVVBEQVRFJyxcbiAgJ1ZPSUNFX1NFVFRJTkdTX1VQREFURScsXG4gICdWT0lDRV9TRVRUSU5HU19VUERBVEVfMicsXG4gICdWT0lDRV9DT05ORUNUSU9OX1NUQVRVUycsXG4gICdTUEVBS0lOR19TVEFSVCcsXG4gICdTUEVBS0lOR19TVE9QJyxcbiAgJ0dBTUVfSk9JTicsXG4gICdHQU1FX1NQRUNUQVRFJyxcbiAgJ0FDVElWSVRZX0pPSU4nLFxuICAnQUNUSVZJVFlfSk9JTl9SRVFVRVNUJyxcbiAgJ0FDVElWSVRZX1NQRUNUQVRFJyxcbiAgJ0FDVElWSVRZX0lOVklURScsXG4gICdOT1RJRklDQVRJT05fQ1JFQVRFJyxcbiAgJ01FU1NBR0VfQ1JFQVRFJyxcbiAgJ01FU1NBR0VfVVBEQVRFJyxcbiAgJ01FU1NBR0VfREVMRVRFJyxcbiAgJ0xPQkJZX0RFTEVURScsXG4gICdMT0JCWV9VUERBVEUnLFxuICAnTE9CQllfTUVNQkVSX0NPTk5FQ1QnLFxuICAnTE9CQllfTUVNQkVSX0RJU0NPTk5FQ1QnLFxuICAnTE9CQllfTUVNQkVSX1VQREFURScsXG4gICdMT0JCWV9NRVNTQUdFJyxcbiAgJ0NBUFRVUkVfU0hPUlRDVVRfQ0hBTkdFJyxcbiAgJ09WRVJMQVknLFxuICAnT1ZFUkxBWV9VUERBVEUnLFxuICAnRU5USVRMRU1FTlRfQ1JFQVRFJyxcbiAgJ0VOVElUTEVNRU5UX0RFTEVURScsXG4gICdVU0VSX0FDSElFVkVNRU5UX1VQREFURScsXG4gICdSRUFEWScsXG4gICdFUlJPUicsXG5dKTtcblxuZXhwb3J0cy5SUENFcnJvcnMgPSB7XG4gIENBUFRVUkVfU0hPUlRDVVRfQUxSRUFEWV9MSVNURU5JTkc6IDUwMDQsXG4gIEdFVF9HVUlMRF9USU1FRF9PVVQ6IDUwMDIsXG4gIElOVkFMSURfQUNUSVZJVFlfSk9JTl9SRVFVRVNUOiA0MDEyLFxuICBJTlZBTElEX0FDVElWSVRZX1NFQ1JFVDogNTAwNSxcbiAgSU5WQUxJRF9DSEFOTkVMOiA0MDA1LFxuICBJTlZBTElEX0NMSUVOVElEOiA0MDA3LFxuICBJTlZBTElEX0NPTU1BTkQ6IDQwMDIsXG4gIElOVkFMSURfRU5USVRMRU1FTlQ6IDQwMTUsXG4gIElOVkFMSURfRVZFTlQ6IDQwMDQsXG4gIElOVkFMSURfR0lGVF9DT0RFOiA0MDE2LFxuICBJTlZBTElEX0dVSUxEOiA0MDAzLFxuICBJTlZBTElEX0lOVklURTogNDAxMSxcbiAgSU5WQUxJRF9MT0JCWTogNDAxMyxcbiAgSU5WQUxJRF9MT0JCWV9TRUNSRVQ6IDQwMTQsXG4gIElOVkFMSURfT1JJR0lOOiA0MDA4LFxuICBJTlZBTElEX1BBWUxPQUQ6IDQwMDAsXG4gIElOVkFMSURfUEVSTUlTU0lPTlM6IDQwMDYsXG4gIElOVkFMSURfVE9LRU46IDQwMDksXG4gIElOVkFMSURfVVNFUjogNDAxMCxcbiAgTE9CQllfRlVMTDogNTAwNyxcbiAgTk9fRUxJR0lCTEVfQUNUSVZJVFk6IDUwMDYsXG4gIE9BVVRIMl9FUlJPUjogNTAwMCxcbiAgUFVSQ0hBU0VfQ0FOQ0VMRUQ6IDUwMDgsXG4gIFBVUkNIQVNFX0VSUk9SOiA1MDA5LFxuICBSQVRFX0xJTUlURUQ6IDUwMTEsXG4gIFNFTEVDVF9DSEFOTkVMX1RJTUVEX09VVDogNTAwMSxcbiAgU0VMRUNUX1ZPSUNFX0ZPUkNFX1JFUVVJUkVEOiA1MDAzLFxuICBTRVJWSUNFX1VOQVZBSUxBQkxFOiAxMDAxLFxuICBUUkFOU0FDVElPTl9BQk9SVEVEOiAxMDAyLFxuICBVTkFVVEhPUklaRURfRk9SX0FDSElFVkVNRU5UOiA1MDEwLFxuICBVTktOT1dOX0VSUk9SOiAxMDAwLFxufTtcblxuZXhwb3J0cy5SUENDbG9zZUNvZGVzID0ge1xuICBDTE9TRV9OT1JNQUw6IDEwMDAsXG4gIENMT1NFX1VOU1VQUE9SVEVEOiAxMDAzLFxuICBDTE9TRV9BQk5PUk1BTDogMTAwNixcbiAgSU5WQUxJRF9DTElFTlRJRDogNDAwMCxcbiAgSU5WQUxJRF9PUklHSU46IDQwMDEsXG4gIFJBVEVMSU1JVEVEOiA0MDAyLFxuICBUT0tFTl9SRVZPS0VEOiA0MDAzLFxuICBJTlZBTElEX1ZFUlNJT046IDQwMDQsXG4gIElOVkFMSURfRU5DT0RJTkc6IDQwMDUsXG59O1xuXG5leHBvcnRzLkxvYmJ5VHlwZXMgPSB7XG4gIFBSSVZBVEU6IDEsXG4gIFBVQkxJQzogMixcbn07XG5cbmV4cG9ydHMuUmVsYXRpb25zaGlwVHlwZXMgPSB7XG4gIE5PTkU6IDAsXG4gIEZSSUVORDogMSxcbiAgQkxPQ0tFRDogMixcbiAgUEVORElOR19JTkNPTUlORzogMyxcbiAgUEVORElOR19PVVRHT0lORzogNCxcbiAgSU1QTElDSVQ6IDUsXG59O1xuIiwiJ3VzZSBzdHJpY3QnO1xuXG5jb25zdCBFdmVudEVtaXR0ZXIgPSByZXF1aXJlKCdldmVudHMnKTtcbmNvbnN0IHsgYnJvd3NlciB9ID0gcmVxdWlyZSgnLi4vY29uc3RhbnRzJyk7XG5cbi8vIGVzbGludC1kaXNhYmxlLW5leHQtbGluZVxuY29uc3QgV2ViU29ja2V0ID0gYnJvd3NlciA/IHdpbmRvdy5XZWJTb2NrZXQgOiByZXF1aXJlKCd3cycpO1xuXG5jb25zdCBwYWNrID0gKGQpID0+IEpTT04uc3RyaW5naWZ5KGQpO1xuY29uc3QgdW5wYWNrID0gKHMpID0+IEpTT04ucGFyc2Uocyk7XG5cbmNsYXNzIFdlYlNvY2tldFRyYW5zcG9ydCBleHRlbmRzIEV2ZW50RW1pdHRlciB7XG4gIGNvbnN0cnVjdG9yKGNsaWVudCkge1xuICAgIHN1cGVyKCk7XG4gICAgdGhpcy5jbGllbnQgPSBjbGllbnQ7XG4gICAgdGhpcy53cyA9IG51bGw7XG4gICAgdGhpcy50cmllcyA9IDA7XG4gIH1cblxuICBhc3luYyBjb25uZWN0KHRyaWVzID0gdGhpcy50cmllcykge1xuICAgIGlmICh0aGlzLmNvbm5lY3RlZCkge1xuICAgICAgcmV0dXJuO1xuICAgIH1cbiAgICBjb25zdCBwb3J0ID0gNjQ2MyArICh0cmllcyAlIDEwKTtcbiAgICB0aGlzLmhvc3RBbmRQb3J0ID0gYDEyNy4wLjAuMToke3BvcnR9YDtcbiAgICBjb25zdCB3cyA9IHRoaXMud3MgPSBuZXcgV2ViU29ja2V0KFxuICAgICAgYHdzOi8vJHt0aGlzLmhvc3RBbmRQb3J0fS8/dj0xJmNsaWVudF9pZD0ke3RoaXMuY2xpZW50LmNsaWVudElkfWAsXG4gICAgICB7XG4gICAgICAgIG9yaWdpbjogdGhpcy5jbGllbnQub3B0aW9ucy5vcmlnaW4sXG4gICAgICB9LFxuICAgICk7XG4gICAgd3Mub25vcGVuID0gdGhpcy5vbk9wZW4uYmluZCh0aGlzKTtcbiAgICB3cy5vbmNsb3NlID0gd3Mub25lcnJvciA9IHRoaXMub25DbG9zZS5iaW5kKHRoaXMpO1xuICAgIHdzLm9ubWVzc2FnZSA9IHRoaXMub25NZXNzYWdlLmJpbmQodGhpcyk7XG4gIH1cblxuICBzZW5kKGRhdGEpIHtcbiAgICBpZiAoIXRoaXMud3MpIHtcbiAgICAgIHJldHVybjtcbiAgICB9XG4gICAgdGhpcy53cy5zZW5kKHBhY2soZGF0YSkpO1xuICB9XG5cbiAgY2xvc2UoKSB7XG4gICAgaWYgKCF0aGlzLndzKSB7XG4gICAgICByZXR1cm47XG4gICAgfVxuICAgIHRoaXMud3MuY2xvc2UoKTtcbiAgfVxuXG4gIHBpbmcoKSB7fSAvLyBlc2xpbnQtZGlzYWJsZS1saW5lIG5vLWVtcHR5LWZ1bmN0aW9uXG5cbiAgb25NZXNzYWdlKGV2ZW50KSB7XG4gICAgdGhpcy5lbWl0KCdtZXNzYWdlJywgdW5wYWNrKGV2ZW50LmRhdGEpKTtcbiAgfVxuXG4gIG9uT3BlbigpIHtcbiAgICB0aGlzLmVtaXQoJ29wZW4nKTtcbiAgfVxuXG4gIG9uQ2xvc2UoZSkge1xuICAgIHRyeSB7XG4gICAgICB0aGlzLndzLmNsb3NlKCk7XG4gICAgfSBjYXRjaCAoZXJyKSB7fSAvLyBlc2xpbnQtZGlzYWJsZS1saW5lIG5vLWVtcHR5XG4gICAgY29uc3QgZGVyciA9IGUuY29kZSA+PSA0MDAwICYmIGUuY29kZSA8IDUwMDA7XG4gICAgaWYgKCFlLmNvZGUgfHwgZGVycikge1xuICAgICAgdGhpcy5lbWl0KCdjbG9zZScsIGUpO1xuICAgIH1cbiAgICBpZiAoIWRlcnIpIHtcbiAgICAgIC8vIGVzbGludC1kaXNhYmxlLW5leHQtbGluZSBuby1wbHVzcGx1c1xuICAgICAgc2V0VGltZW91dCgoKSA9PiB0aGlzLmNvbm5lY3QoZS5jb2RlID09PSAxMDA2ID8gKyt0aGlzLnRyaWVzIDogMCksIDI1MCk7XG4gICAgfVxuICB9XG59XG5cbm1vZHVsZS5leHBvcnRzID0gV2ViU29ja2V0VHJhbnNwb3J0O1xuIiwiJ3VzZSBzdHJpY3QnO1xuXG5tb2R1bGUuZXhwb3J0cyA9IHtcbiAgaXBjOiByZXF1aXJlKCcuL2lwYycpLFxuICB3ZWJzb2NrZXQ6IHJlcXVpcmUoJy4vd2Vic29ja2V0JyksXG59O1xuIiwiJ3VzZSBzdHJpY3QnO1xuXG5jb25zdCBFdmVudEVtaXR0ZXIgPSByZXF1aXJlKCdldmVudHMnKTtcbmNvbnN0IHsgc2V0VGltZW91dCwgY2xlYXJUaW1lb3V0IH0gPSByZXF1aXJlKCd0aW1lcnMnKTtcbmNvbnN0IGZldGNoID0gcmVxdWlyZSgnbm9kZS1mZXRjaCcpO1xuY29uc3QgdHJhbnNwb3J0cyA9IHJlcXVpcmUoJy4vdHJhbnNwb3J0cycpO1xuY29uc3QgeyBSUENDb21tYW5kcywgUlBDRXZlbnRzLCBSZWxhdGlvbnNoaXBUeXBlcyB9ID0gcmVxdWlyZSgnLi9jb25zdGFudHMnKTtcbmNvbnN0IHsgcGlkOiBnZXRQaWQsIHV1aWQgfSA9IHJlcXVpcmUoJy4vdXRpbCcpO1xuXG5mdW5jdGlvbiBzdWJLZXkoZXZlbnQsIGFyZ3MpIHtcbiAgcmV0dXJuIGAke2V2ZW50fSR7SlNPTi5zdHJpbmdpZnkoYXJncyl9YDtcbn1cblxuLyoqXG4gKiBAdHlwZWRlZiB7UlBDQ2xpZW50T3B0aW9uc31cbiAqIEBleHRlbmRzIHtDbGllbnRPcHRpb25zfVxuICogQHByb3Age3N0cmluZ30gdHJhbnNwb3J0IFJQQyB0cmFuc3BvcnQuIG9uZSBvZiBgaXBjYCBvciBgd2Vic29ja2V0YFxuICovXG5cbi8qKlxuICogVGhlIG1haW4gaHViIGZvciBpbnRlcmFjdGluZyB3aXRoIERpc2NvcmQgUlBDXG4gKiBAZXh0ZW5kcyB7QmFzZUNsaWVudH1cbiAqL1xuY2xhc3MgUlBDQ2xpZW50IGV4dGVuZHMgRXZlbnRFbWl0dGVyIHtcbiAgLyoqXG4gICAqIEBwYXJhbSB7UlBDQ2xpZW50T3B0aW9uc30gW29wdGlvbnNdIE9wdGlvbnMgZm9yIHRoZSBjbGllbnQuXG4gICAqIFlvdSBtdXN0IHByb3ZpZGUgYSB0cmFuc3BvcnRcbiAgICovXG4gIGNvbnN0cnVjdG9yKG9wdGlvbnMgPSB7fSkge1xuICAgIHN1cGVyKCk7XG5cbiAgICB0aGlzLm9wdGlvbnMgPSBvcHRpb25zO1xuXG4gICAgdGhpcy5hY2Nlc3NUb2tlbiA9IG51bGw7XG4gICAgdGhpcy5jbGllbnRJZCA9IG51bGw7XG5cbiAgICAvKipcbiAgICAgKiBBcHBsaWNhdGlvbiB1c2VkIGluIHRoaXMgY2xpZW50XG4gICAgICogQHR5cGUgez9DbGllbnRBcHBsaWNhdGlvbn1cbiAgICAgKi9cbiAgICB0aGlzLmFwcGxpY2F0aW9uID0gbnVsbDtcblxuICAgIC8qKlxuICAgICAqIFVzZXIgdXNlZCBpbiB0aGlzIGFwcGxpY2F0aW9uXG4gICAgICogQHR5cGUgez9Vc2VyfVxuICAgICAqL1xuICAgIHRoaXMudXNlciA9IG51bGw7XG5cbiAgICBjb25zdCBUcmFuc3BvcnQgPSB0cmFuc3BvcnRzW29wdGlvbnMudHJhbnNwb3J0XTtcbiAgICBpZiAoIVRyYW5zcG9ydCkge1xuICAgICAgdGhyb3cgbmV3IFR5cGVFcnJvcignUlBDX0lOVkFMSURfVFJBTlNQT1JUJywgb3B0aW9ucy50cmFuc3BvcnQpO1xuICAgIH1cblxuICAgIHRoaXMuZmV0Y2ggPSAobWV0aG9kLCBwYXRoLCB7IGRhdGEsIHF1ZXJ5IH0gPSB7fSkgPT5cbiAgICAgIGZldGNoKGAke3RoaXMuZmV0Y2guZW5kcG9pbnR9JHtwYXRofSR7cXVlcnkgPyBuZXcgVVJMU2VhcmNoUGFyYW1zKHF1ZXJ5KSA6ICcnfWAsIHtcbiAgICAgICAgbWV0aG9kLFxuICAgICAgICBib2R5OiBkYXRhLFxuICAgICAgICBoZWFkZXJzOiB7XG4gICAgICAgICAgQXV0aG9yaXphdGlvbjogYEJlYXJlciAke3RoaXMuYWNjZXNzVG9rZW59YCxcbiAgICAgICAgfSxcbiAgICAgIH0pLnRoZW4oYXN5bmMgKHIpID0+IHtcbiAgICAgICAgY29uc3QgYm9keSA9IGF3YWl0IHIuanNvbigpO1xuICAgICAgICBpZiAoIXIub2spIHtcbiAgICAgICAgICBjb25zdCBlID0gbmV3IEVycm9yKHIuc3RhdHVzKTtcbiAgICAgICAgICBlLmJvZHkgPSBib2R5O1xuICAgICAgICAgIHRocm93IGU7XG4gICAgICAgIH1cbiAgICAgICAgcmV0dXJuIGJvZHk7XG4gICAgICB9KTtcblxuICAgIHRoaXMuZmV0Y2guZW5kcG9pbnQgPSAnaHR0cHM6Ly9kaXNjb3JkLmNvbS9hcGknO1xuXG4gICAgLyoqXG4gICAgICogUmF3IHRyYW5zcG9ydCB1c2VyZFxuICAgICAqIEB0eXBlIHtSUENUcmFuc3BvcnR9XG4gICAgICogQHByaXZhdGVcbiAgICAgKi9cbiAgICB0aGlzLnRyYW5zcG9ydCA9IG5ldyBUcmFuc3BvcnQodGhpcyk7XG4gICAgdGhpcy50cmFuc3BvcnQub24oJ21lc3NhZ2UnLCB0aGlzLl9vblJwY01lc3NhZ2UuYmluZCh0aGlzKSk7XG5cbiAgICAvKipcbiAgICAgKiBNYXAgb2Ygbm9uY2VzIGJlaW5nIGV4cGVjdGVkIGZyb20gdGhlIHRyYW5zcG9ydFxuICAgICAqIEB0eXBlIHtNYXB9XG4gICAgICogQHByaXZhdGVcbiAgICAgKi9cbiAgICB0aGlzLl9leHBlY3RpbmcgPSBuZXcgTWFwKCk7XG5cbiAgICAvKipcbiAgICAgKiBNYXAgb2YgY3VycmVudCBzdWJzY3JpcHRpb25zXG4gICAgICogQHR5cGUge01hcH1cbiAgICAgKiBAcHJpdmF0ZVxuICAgICAqL1xuICAgIHRoaXMuX3N1YnNjcmlwdGlvbnMgPSBuZXcgTWFwKCk7XG5cbiAgICB0aGlzLl9jb25uZWN0UHJvbWlzZSA9IHVuZGVmaW5lZDtcbiAgfVxuXG4gIC8qKlxuICAgKiBTZWFyY2ggYW5kIGNvbm5lY3QgdG8gUlBDXG4gICAqL1xuICBjb25uZWN0KGNsaWVudElkKSB7XG4gICAgaWYgKHRoaXMuX2Nvbm5lY3RQcm9taXNlKSB7XG4gICAgICByZXR1cm4gdGhpcy5fY29ubmVjdFByb21pc2U7XG4gICAgfVxuICAgIHRoaXMuX2Nvbm5lY3RQcm9taXNlID0gbmV3IFByb21pc2UoKHJlc29sdmUsIHJlamVjdCkgPT4ge1xuICAgICAgdGhpcy5jbGllbnRJZCA9IGNsaWVudElkO1xuICAgICAgY29uc3QgdGltZW91dCA9IHNldFRpbWVvdXQoKCkgPT4gcmVqZWN0KG5ldyBFcnJvcignUlBDX0NPTk5FQ1RJT05fVElNRU9VVCcpKSwgMTBlMyk7XG4gICAgICB0aW1lb3V0LnVucmVmKCk7XG4gICAgICB0aGlzLm9uY2UoJ2Nvbm5lY3RlZCcsICgpID0+IHtcbiAgICAgICAgY2xlYXJUaW1lb3V0KHRpbWVvdXQpO1xuICAgICAgICByZXNvbHZlKHRoaXMpO1xuICAgICAgfSk7XG4gICAgICB0aGlzLnRyYW5zcG9ydC5vbmNlKCdjbG9zZScsICgpID0+IHtcbiAgICAgICAgdGhpcy5fZXhwZWN0aW5nLmZvckVhY2goKGUpID0+IHtcbiAgICAgICAgICBlLnJlamVjdChuZXcgRXJyb3IoJ2Nvbm5lY3Rpb24gY2xvc2VkJykpO1xuICAgICAgICB9KTtcbiAgICAgICAgdGhpcy5lbWl0KCdkaXNjb25uZWN0ZWQnKTtcbiAgICAgICAgcmVqZWN0KG5ldyBFcnJvcignY29ubmVjdGlvbiBjbG9zZWQnKSk7XG4gICAgICB9KTtcbiAgICAgIHRoaXMudHJhbnNwb3J0LmNvbm5lY3QoKS5jYXRjaChyZWplY3QpO1xuICAgIH0pO1xuICAgIHJldHVybiB0aGlzLl9jb25uZWN0UHJvbWlzZTtcbiAgfVxuXG4gIC8qKlxuICAgKiBAdHlwZWRlZiB7UlBDTG9naW5PcHRpb25zfVxuICAgKiBAcGFyYW0ge3N0cmluZ30gY2xpZW50SWQgQ2xpZW50IElEXG4gICAqIEBwYXJhbSB7c3RyaW5nfSBbY2xpZW50U2VjcmV0XSBDbGllbnQgc2VjcmV0XG4gICAqIEBwYXJhbSB7c3RyaW5nfSBbYWNjZXNzVG9rZW5dIEFjY2VzcyB0b2tlblxuICAgKiBAcGFyYW0ge3N0cmluZ30gW3JwY1Rva2VuXSBSUEMgdG9rZW5cbiAgICogQHBhcmFtIHtzdHJpbmd9IFt0b2tlbkVuZHBvaW50XSBUb2tlbiBlbmRwb2ludFxuICAgKiBAcGFyYW0ge3N0cmluZ1tdfSBbc2NvcGVzXSBTY29wZXMgdG8gYXV0aG9yaXplIHdpdGhcbiAgICovXG5cbiAgLyoqXG4gICAqIFBlcmZvcm1zIGF1dGhlbnRpY2F0aW9uIGZsb3cuIEF1dG9tYXRpY2FsbHkgY2FsbHMgQ2xpZW50I2Nvbm5lY3QgaWYgbmVlZGVkLlxuICAgKiBAcGFyYW0ge1JQQ0xvZ2luT3B0aW9uc30gb3B0aW9ucyBPcHRpb25zIGZvciBhdXRoZW50aWNhdGlvbi5cbiAgICogQXQgbGVhc3Qgb25lIHByb3BlcnR5IG11c3QgYmUgcHJvdmlkZWQgdG8gcGVyZm9ybSBsb2dpbi5cbiAgICogQGV4YW1wbGUgY2xpZW50LmxvZ2luKHsgY2xpZW50SWQ6ICcxMjM0NTY3JywgY2xpZW50U2VjcmV0OiAnYWJjZGVmMTIzJyB9KTtcbiAgICogQHJldHVybnMge1Byb21pc2U8UlBDQ2xpZW50Pn1cbiAgICovXG4gIGFzeW5jIGxvZ2luKG9wdGlvbnMgPSB7fSkge1xuICAgIGxldCB7IGNsaWVudElkLCBhY2Nlc3NUb2tlbiB9ID0gb3B0aW9ucztcbiAgICBhd2FpdCB0aGlzLmNvbm5lY3QoY2xpZW50SWQpO1xuICAgIGlmICghb3B0aW9ucy5zY29wZXMpIHtcbiAgICAgIHRoaXMuZW1pdCgncmVhZHknKTtcbiAgICAgIHJldHVybiB0aGlzO1xuICAgIH1cbiAgICBpZiAoIWFjY2Vzc1Rva2VuKSB7XG4gICAgICBhY2Nlc3NUb2tlbiA9IGF3YWl0IHRoaXMuYXV0aG9yaXplKG9wdGlvbnMpO1xuICAgIH1cbiAgICByZXR1cm4gdGhpcy5hdXRoZW50aWNhdGUoYWNjZXNzVG9rZW4pO1xuICB9XG5cbiAgLyoqXG4gICAqIFJlcXVlc3RcbiAgICogQHBhcmFtIHtzdHJpbmd9IGNtZCBDb21tYW5kXG4gICAqIEBwYXJhbSB7T2JqZWN0fSBbYXJncz17fV0gQXJndW1lbnRzXG4gICAqIEBwYXJhbSB7c3RyaW5nfSBbZXZ0XSBFdmVudFxuICAgKiBAcmV0dXJucyB7UHJvbWlzZX1cbiAgICogQHByaXZhdGVcbiAgICovXG4gIHJlcXVlc3QoY21kLCBhcmdzLCBldnQpIHtcbiAgICByZXR1cm4gbmV3IFByb21pc2UoKHJlc29sdmUsIHJlamVjdCkgPT4ge1xuICAgICAgY29uc3Qgbm9uY2UgPSB1dWlkKCk7XG4gICAgICB0aGlzLnRyYW5zcG9ydC5zZW5kKHsgY21kLCBhcmdzLCBldnQsIG5vbmNlIH0pO1xuICAgICAgdGhpcy5fZXhwZWN0aW5nLnNldChub25jZSwgeyByZXNvbHZlLCByZWplY3QgfSk7XG4gICAgfSk7XG4gIH1cblxuICAvKipcbiAgICogTWVzc2FnZSBoYW5kbGVyXG4gICAqIEBwYXJhbSB7T2JqZWN0fSBtZXNzYWdlIG1lc3NhZ2VcbiAgICogQHByaXZhdGVcbiAgICovXG4gIF9vblJwY01lc3NhZ2UobWVzc2FnZSkge1xuICAgIGlmIChtZXNzYWdlLmNtZCA9PT0gUlBDQ29tbWFuZHMuRElTUEFUQ0ggJiYgbWVzc2FnZS5ldnQgPT09IFJQQ0V2ZW50cy5SRUFEWSkge1xuICAgICAgaWYgKG1lc3NhZ2UuZGF0YS51c2VyKSB7XG4gICAgICAgIHRoaXMudXNlciA9IG1lc3NhZ2UuZGF0YS51c2VyO1xuICAgICAgfVxuICAgICAgdGhpcy5lbWl0KCdjb25uZWN0ZWQnKTtcbiAgICB9IGVsc2UgaWYgKHRoaXMuX2V4cGVjdGluZy5oYXMobWVzc2FnZS5ub25jZSkpIHtcbiAgICAgIGNvbnN0IHsgcmVzb2x2ZSwgcmVqZWN0IH0gPSB0aGlzLl9leHBlY3RpbmcuZ2V0KG1lc3NhZ2Uubm9uY2UpO1xuICAgICAgaWYgKG1lc3NhZ2UuZXZ0ID09PSAnRVJST1InKSB7XG4gICAgICAgIGNvbnN0IGUgPSBuZXcgRXJyb3IobWVzc2FnZS5kYXRhLm1lc3NhZ2UpO1xuICAgICAgICBlLmNvZGUgPSBtZXNzYWdlLmRhdGEuY29kZTtcbiAgICAgICAgZS5kYXRhID0gbWVzc2FnZS5kYXRhO1xuICAgICAgICByZWplY3QoZSk7XG4gICAgICB9IGVsc2Uge1xuICAgICAgICByZXNvbHZlKG1lc3NhZ2UuZGF0YSk7XG4gICAgICB9XG4gICAgICB0aGlzLl9leHBlY3RpbmcuZGVsZXRlKG1lc3NhZ2Uubm9uY2UpO1xuICAgIH0gZWxzZSB7XG4gICAgICBjb25zdCBzdWJpZCA9IHN1YktleShtZXNzYWdlLmV2dCwgbWVzc2FnZS5hcmdzKTtcbiAgICAgIGlmICghdGhpcy5fc3Vic2NyaXB0aW9ucy5oYXMoc3ViaWQpKSB7XG4gICAgICAgIHJldHVybjtcbiAgICAgIH1cbiAgICAgIHRoaXMuX3N1YnNjcmlwdGlvbnMuZ2V0KHN1YmlkKShtZXNzYWdlLmRhdGEpO1xuICAgIH1cbiAgfVxuXG4gIC8qKlxuICAgKiBBdXRob3JpemVcbiAgICogQHBhcmFtIHtPYmplY3R9IG9wdGlvbnMgb3B0aW9uc1xuICAgKiBAcmV0dXJucyB7UHJvbWlzZX1cbiAgICogQHByaXZhdGVcbiAgICovXG4gIGFzeW5jIGF1dGhvcml6ZSh7IHNjb3BlcywgY2xpZW50U2VjcmV0LCBycGNUb2tlbiwgcmVkaXJlY3RVcmkgfSA9IHt9KSB7XG4gICAgaWYgKGNsaWVudFNlY3JldCAmJiBycGNUb2tlbiA9PT0gdHJ1ZSkge1xuICAgICAgY29uc3QgYm9keSA9IGF3YWl0IHRoaXMuZmV0Y2goJ1BPU1QnLCAnL29hdXRoMi90b2tlbi9ycGMnLCB7XG4gICAgICAgIGRhdGE6IG5ldyBVUkxTZWFyY2hQYXJhbXMoe1xuICAgICAgICAgIGNsaWVudF9pZDogdGhpcy5jbGllbnRJZCxcbiAgICAgICAgICBjbGllbnRfc2VjcmV0OiBjbGllbnRTZWNyZXQsXG4gICAgICAgIH0pLFxuICAgICAgfSk7XG4gICAgICBycGNUb2tlbiA9IGJvZHkucnBjX3Rva2VuO1xuICAgIH1cblxuICAgIGNvbnN0IHsgY29kZSB9ID0gYXdhaXQgdGhpcy5yZXF1ZXN0KCdBVVRIT1JJWkUnLCB7XG4gICAgICBzY29wZXMsXG4gICAgICBjbGllbnRfaWQ6IHRoaXMuY2xpZW50SWQsXG4gICAgICBycGNfdG9rZW46IHJwY1Rva2VuLFxuICAgIH0pO1xuXG4gICAgY29uc3QgcmVzcG9uc2UgPSBhd2FpdCB0aGlzLmZldGNoKCdQT1NUJywgJy9vYXV0aDIvdG9rZW4nLCB7XG4gICAgICBkYXRhOiBuZXcgVVJMU2VhcmNoUGFyYW1zKHtcbiAgICAgICAgY2xpZW50X2lkOiB0aGlzLmNsaWVudElkLFxuICAgICAgICBjbGllbnRfc2VjcmV0OiBjbGllbnRTZWNyZXQsXG4gICAgICAgIGNvZGUsXG4gICAgICAgIGdyYW50X3R5cGU6ICdhdXRob3JpemF0aW9uX2NvZGUnLFxuICAgICAgICByZWRpcmVjdF91cmk6IHJlZGlyZWN0VXJpLFxuICAgICAgfSksXG4gICAgfSk7XG5cbiAgICByZXR1cm4gcmVzcG9uc2UuYWNjZXNzX3Rva2VuO1xuICB9XG5cbiAgLyoqXG4gICAqIEF1dGhlbnRpY2F0ZVxuICAgKiBAcGFyYW0ge3N0cmluZ30gYWNjZXNzVG9rZW4gYWNjZXNzIHRva2VuXG4gICAqIEByZXR1cm5zIHtQcm9taXNlfVxuICAgKiBAcHJpdmF0ZVxuICAgKi9cbiAgYXV0aGVudGljYXRlKGFjY2Vzc1Rva2VuKSB7XG4gICAgcmV0dXJuIHRoaXMucmVxdWVzdCgnQVVUSEVOVElDQVRFJywgeyBhY2Nlc3NfdG9rZW46IGFjY2Vzc1Rva2VuIH0pXG4gICAgICAudGhlbigoeyBhcHBsaWNhdGlvbiwgdXNlciB9KSA9PiB7XG4gICAgICAgIHRoaXMuYWNjZXNzVG9rZW4gPSBhY2Nlc3NUb2tlbjtcbiAgICAgICAgdGhpcy5hcHBsaWNhdGlvbiA9IGFwcGxpY2F0aW9uO1xuICAgICAgICB0aGlzLnVzZXIgPSB1c2VyO1xuICAgICAgICB0aGlzLmVtaXQoJ3JlYWR5Jyk7XG4gICAgICAgIHJldHVybiB0aGlzO1xuICAgICAgfSk7XG4gIH1cblxuXG4gIC8qKlxuICAgKiBGZXRjaCBhIGd1aWxkXG4gICAqIEBwYXJhbSB7U25vd2ZsYWtlfSBpZCBHdWlsZCBJRFxuICAgKiBAcGFyYW0ge251bWJlcn0gW3RpbWVvdXRdIFRpbWVvdXQgcmVxdWVzdFxuICAgKiBAcmV0dXJucyB7UHJvbWlzZTxHdWlsZD59XG4gICAqL1xuICBnZXRHdWlsZChpZCwgdGltZW91dCkge1xuICAgIHJldHVybiB0aGlzLnJlcXVlc3QoUlBDQ29tbWFuZHMuR0VUX0dVSUxELCB7IGd1aWxkX2lkOiBpZCwgdGltZW91dCB9KTtcbiAgfVxuXG4gIC8qKlxuICAgKiBGZXRjaCBhbGwgZ3VpbGRzXG4gICAqIEBwYXJhbSB7bnVtYmVyfSBbdGltZW91dF0gVGltZW91dCByZXF1ZXN0XG4gICAqIEByZXR1cm5zIHtQcm9taXNlPENvbGxlY3Rpb248U25vd2ZsYWtlLCBHdWlsZD4+fVxuICAgKi9cbiAgZ2V0R3VpbGRzKHRpbWVvdXQpIHtcbiAgICByZXR1cm4gdGhpcy5yZXF1ZXN0KFJQQ0NvbW1hbmRzLkdFVF9HVUlMRFMsIHsgdGltZW91dCB9KTtcbiAgfVxuXG4gIC8qKlxuICAgKiBHZXQgYSBjaGFubmVsXG4gICAqIEBwYXJhbSB7U25vd2ZsYWtlfSBpZCBDaGFubmVsIElEXG4gICAqIEBwYXJhbSB7bnVtYmVyfSBbdGltZW91dF0gVGltZW91dCByZXF1ZXN0XG4gICAqIEByZXR1cm5zIHtQcm9taXNlPENoYW5uZWw+fVxuICAgKi9cbiAgZ2V0Q2hhbm5lbChpZCwgdGltZW91dCkge1xuICAgIHJldHVybiB0aGlzLnJlcXVlc3QoUlBDQ29tbWFuZHMuR0VUX0NIQU5ORUwsIHsgY2hhbm5lbF9pZDogaWQsIHRpbWVvdXQgfSk7XG4gIH1cblxuICAvKipcbiAgICogR2V0IGFsbCBjaGFubmVsc1xuICAgKiBAcGFyYW0ge1Nub3dmbGFrZX0gW2lkXSBHdWlsZCBJRFxuICAgKiBAcGFyYW0ge251bWJlcn0gW3RpbWVvdXRdIFRpbWVvdXQgcmVxdWVzdFxuICAgKiBAcmV0dXJucyB7UHJvbWlzZTxDb2xsZWN0aW9uPFNub3dmbGFrZSwgQ2hhbm5lbD4+fVxuICAgKi9cbiAgYXN5bmMgZ2V0Q2hhbm5lbHMoaWQsIHRpbWVvdXQpIHtcbiAgICBjb25zdCB7IGNoYW5uZWxzIH0gPSBhd2FpdCB0aGlzLnJlcXVlc3QoUlBDQ29tbWFuZHMuR0VUX0NIQU5ORUxTLCB7XG4gICAgICB0aW1lb3V0LFxuICAgICAgZ3VpbGRfaWQ6IGlkLFxuICAgIH0pO1xuICAgIHJldHVybiBjaGFubmVscztcbiAgfVxuXG4gIC8qKlxuICAgKiBAdHlwZWRlZiB7Q2VydGlmaWVkRGV2aWNlfVxuICAgKiBAcHJvcCB7c3RyaW5nfSB0eXBlIE9uZSBvZiBgQVVESU9fSU5QVVRgLCBgQVVESU9fT1VUUFVUYCwgYFZJREVPX0lOUFVUYFxuICAgKiBAcHJvcCB7c3RyaW5nfSB1dWlkIFRoaXMgZGV2aWNlJ3MgV2luZG93cyBVVUlEXG4gICAqIEBwcm9wIHtvYmplY3R9IHZlbmRvciBWZW5kb3IgaW5mb3JtYXRpb25cbiAgICogQHByb3Age3N0cmluZ30gdmVuZG9yLm5hbWUgVmVuZG9yJ3MgbmFtZVxuICAgKiBAcHJvcCB7c3RyaW5nfSB2ZW5kb3IudXJsIFZlbmRvcidzIHVybFxuICAgKiBAcHJvcCB7b2JqZWN0fSBtb2RlbCBNb2RlbCBpbmZvcm1hdGlvblxuICAgKiBAcHJvcCB7c3RyaW5nfSBtb2RlbC5uYW1lIE1vZGVsJ3MgbmFtZVxuICAgKiBAcHJvcCB7c3RyaW5nfSBtb2RlbC51cmwgTW9kZWwncyB1cmxcbiAgICogQHByb3Age3N0cmluZ1tdfSByZWxhdGVkIEFycmF5IG9mIHJlbGF0ZWQgcHJvZHVjdCdzIFdpbmRvd3MgVVVJRHNcbiAgICogQHByb3Age2Jvb2xlYW59IGVjaG9DYW5jZWxsYXRpb24gSWYgdGhlIGRldmljZSBoYXMgZWNobyBjYW5jZWxsYXRpb25cbiAgICogQHByb3Age2Jvb2xlYW59IG5vaXNlU3VwcHJlc3Npb24gSWYgdGhlIGRldmljZSBoYXMgbm9pc2Ugc3VwcHJlc3Npb25cbiAgICogQHByb3Age2Jvb2xlYW59IGF1dG9tYXRpY0dhaW5Db250cm9sIElmIHRoZSBkZXZpY2UgaGFzIGF1dG9tYXRpYyBnYWluIGNvbnRyb2xcbiAgICogQHByb3Age2Jvb2xlYW59IGhhcmR3YXJlTXV0ZSBJZiB0aGUgZGV2aWNlIGhhcyBhIGhhcmR3YXJlIG11dGVcbiAgICovXG5cbiAgLyoqXG4gICAqIFRlbGwgZGlzY29yZCB3aGljaCBkZXZpY2VzIGFyZSBjZXJ0aWZpZWRcbiAgICogQHBhcmFtIHtDZXJ0aWZpZWREZXZpY2VbXX0gZGV2aWNlcyBDZXJ0aWZpZWQgZGV2aWNlcyB0byBzZW5kIHRvIGRpc2NvcmRcbiAgICogQHJldHVybnMge1Byb21pc2V9XG4gICAqL1xuICBzZXRDZXJ0aWZpZWREZXZpY2VzKGRldmljZXMpIHtcbiAgICByZXR1cm4gdGhpcy5yZXF1ZXN0KFJQQ0NvbW1hbmRzLlNFVF9DRVJUSUZJRURfREVWSUNFUywge1xuICAgICAgZGV2aWNlczogZGV2aWNlcy5tYXAoKGQpID0+ICh7XG4gICAgICAgIHR5cGU6IGQudHlwZSxcbiAgICAgICAgaWQ6IGQudXVpZCxcbiAgICAgICAgdmVuZG9yOiBkLnZlbmRvcixcbiAgICAgICAgbW9kZWw6IGQubW9kZWwsXG4gICAgICAgIHJlbGF0ZWQ6IGQucmVsYXRlZCxcbiAgICAgICAgZWNob19jYW5jZWxsYXRpb246IGQuZWNob0NhbmNlbGxhdGlvbixcbiAgICAgICAgbm9pc2Vfc3VwcHJlc3Npb246IGQubm9pc2VTdXBwcmVzc2lvbixcbiAgICAgICAgYXV0b21hdGljX2dhaW5fY29udHJvbDogZC5hdXRvbWF0aWNHYWluQ29udHJvbCxcbiAgICAgICAgaGFyZHdhcmVfbXV0ZTogZC5oYXJkd2FyZU11dGUsXG4gICAgICB9KSksXG4gICAgfSk7XG4gIH1cblxuICAvKipcbiAgICogQHR5cGVkZWYge1VzZXJWb2ljZVNldHRpbmdzfVxuICAgKiBAcHJvcCB7U25vd2ZsYWtlfSBpZCBJRCBvZiB0aGUgdXNlciB0aGVzZSBzZXR0aW5ncyBhcHBseSB0b1xuICAgKiBAcHJvcCB7P09iamVjdH0gW3Bhbl0gUGFuIHNldHRpbmdzLCBhbiBvYmplY3Qgd2l0aCBgbGVmdGAgYW5kIGByaWdodGAgc2V0IGJldHdlZW5cbiAgICogMC4wIGFuZCAxLjAsIGluY2x1c2l2ZVxuICAgKiBAcHJvcCB7P251bWJlcn0gW3ZvbHVtZT0xMDBdIFRoZSB2b2x1bWVcbiAgICogQHByb3Age2Jvb2x9IFttdXRlXSBJZiB0aGUgdXNlciBpcyBtdXRlZFxuICAgKi9cblxuICAvKipcbiAgICogU2V0IHRoZSB2b2ljZSBzZXR0aW5ncyBmb3IgYSB1ZXIsIGJ5IGlkXG4gICAqIEBwYXJhbSB7U25vd2ZsYWtlfSBpZCBJRCBvZiB0aGUgdXNlciB0byBzZXRcbiAgICogQHBhcmFtIHtVc2VyVm9pY2VTZXR0aW5nc30gc2V0dGluZ3MgU2V0dGluZ3NcbiAgICogQHJldHVybnMge1Byb21pc2V9XG4gICAqL1xuICBzZXRVc2VyVm9pY2VTZXR0aW5ncyhpZCwgc2V0dGluZ3MpIHtcbiAgICByZXR1cm4gdGhpcy5yZXF1ZXN0KFJQQ0NvbW1hbmRzLlNFVF9VU0VSX1ZPSUNFX1NFVFRJTkdTLCB7XG4gICAgICB1c2VyX2lkOiBpZCxcbiAgICAgIHBhbjogc2V0dGluZ3MucGFuLFxuICAgICAgbXV0ZTogc2V0dGluZ3MubXV0ZSxcbiAgICAgIHZvbHVtZTogc2V0dGluZ3Mudm9sdW1lLFxuICAgIH0pO1xuICB9XG5cbiAgLyoqXG4gICAqIE1vdmUgdGhlIHVzZXIgdG8gYSB2b2ljZSBjaGFubmVsXG4gICAqIEBwYXJhbSB7U25vd2ZsYWtlfSBpZCBJRCBvZiB0aGUgdm9pY2UgY2hhbm5lbFxuICAgKiBAcGFyYW0ge09iamVjdH0gW29wdGlvbnNdIE9wdGlvbnNcbiAgICogQHBhcmFtIHtudW1iZXJ9IFtvcHRpb25zLnRpbWVvdXRdIFRpbWVvdXQgZm9yIHRoZSBjb21tYW5kXG4gICAqIEBwYXJhbSB7Ym9vbGVhbn0gW29wdGlvbnMuZm9yY2VdIEZvcmNlIHRoaXMgbW92ZS4gVGhpcyBzaG91bGQgb25seSBiZSBkb25lIGlmIHlvdVxuICAgKiBoYXZlIGV4cGxpY2l0IHBlcm1pc3Npb24gZnJvbSB0aGUgdXNlci5cbiAgICogQHJldHVybnMge1Byb21pc2V9XG4gICAqL1xuICBzZWxlY3RWb2ljZUNoYW5uZWwoaWQsIHsgdGltZW91dCwgZm9yY2UgPSBmYWxzZSB9ID0ge30pIHtcbiAgICByZXR1cm4gdGhpcy5yZXF1ZXN0KFJQQ0NvbW1hbmRzLlNFTEVDVF9WT0lDRV9DSEFOTkVMLCB7IGNoYW5uZWxfaWQ6IGlkLCB0aW1lb3V0LCBmb3JjZSB9KTtcbiAgfVxuXG4gIC8qKlxuICAgKiBNb3ZlIHRoZSB1c2VyIHRvIGEgdGV4dCBjaGFubmVsXG4gICAqIEBwYXJhbSB7U25vd2ZsYWtlfSBpZCBJRCBvZiB0aGUgdm9pY2UgY2hhbm5lbFxuICAgKiBAcGFyYW0ge09iamVjdH0gW29wdGlvbnNdIE9wdGlvbnNcbiAgICogQHBhcmFtIHtudW1iZXJ9IFtvcHRpb25zLnRpbWVvdXRdIFRpbWVvdXQgZm9yIHRoZSBjb21tYW5kXG4gICAqIEBwYXJhbSB7Ym9vbGVhbn0gW29wdGlvbnMuZm9yY2VdIEZvcmNlIHRoaXMgbW92ZS4gVGhpcyBzaG91bGQgb25seSBiZSBkb25lIGlmIHlvdVxuICAgKiBoYXZlIGV4cGxpY2l0IHBlcm1pc3Npb24gZnJvbSB0aGUgdXNlci5cbiAgICogQHJldHVybnMge1Byb21pc2V9XG4gICAqL1xuICBzZWxlY3RUZXh0Q2hhbm5lbChpZCwgeyB0aW1lb3V0LCBmb3JjZSA9IGZhbHNlIH0gPSB7fSkge1xuICAgIHJldHVybiB0aGlzLnJlcXVlc3QoUlBDQ29tbWFuZHMuU0VMRUNUX1RFWFRfQ0hBTk5FTCwgeyBjaGFubmVsX2lkOiBpZCwgdGltZW91dCwgZm9yY2UgfSk7XG4gIH1cblxuICAvKipcbiAgICogR2V0IGN1cnJlbnQgdm9pY2Ugc2V0dGluZ3NcbiAgICogQHJldHVybnMge1Byb21pc2V9XG4gICAqL1xuICBnZXRWb2ljZVNldHRpbmdzKCkge1xuICAgIHJldHVybiB0aGlzLnJlcXVlc3QoUlBDQ29tbWFuZHMuR0VUX1ZPSUNFX1NFVFRJTkdTKVxuICAgICAgLnRoZW4oKHMpID0+ICh7XG4gICAgICAgIGF1dG9tYXRpY0dhaW5Db250cm9sOiBzLmF1dG9tYXRpY19nYWluX2NvbnRyb2wsXG4gICAgICAgIGVjaG9DYW5jZWxsYXRpb246IHMuZWNob19jYW5jZWxsYXRpb24sXG4gICAgICAgIG5vaXNlU3VwcHJlc3Npb246IHMubm9pc2Vfc3VwcHJlc3Npb24sXG4gICAgICAgIHFvczogcy5xb3MsXG4gICAgICAgIHNpbGVuY2VXYXJuaW5nOiBzLnNpbGVuY2Vfd2FybmluZyxcbiAgICAgICAgZGVhZjogcy5kZWFmLFxuICAgICAgICBtdXRlOiBzLm11dGUsXG4gICAgICAgIGlucHV0OiB7XG4gICAgICAgICAgYXZhaWxhYmxlRGV2aWNlczogcy5pbnB1dC5hdmFpbGFibGVfZGV2aWNlcyxcbiAgICAgICAgICBkZXZpY2U6IHMuaW5wdXQuZGV2aWNlX2lkLFxuICAgICAgICAgIHZvbHVtZTogcy5pbnB1dC52b2x1bWUsXG4gICAgICAgIH0sXG4gICAgICAgIG91dHB1dDoge1xuICAgICAgICAgIGF2YWlsYWJsZURldmljZXM6IHMub3V0cHV0LmF2YWlsYWJsZV9kZXZpY2VzLFxuICAgICAgICAgIGRldmljZTogcy5vdXRwdXQuZGV2aWNlX2lkLFxuICAgICAgICAgIHZvbHVtZTogcy5vdXRwdXQudm9sdW1lLFxuICAgICAgICB9LFxuICAgICAgICBtb2RlOiB7XG4gICAgICAgICAgdHlwZTogcy5tb2RlLnR5cGUsXG4gICAgICAgICAgYXV0b1RocmVzaG9sZDogcy5tb2RlLmF1dG9fdGhyZXNob2xkLFxuICAgICAgICAgIHRocmVzaG9sZDogcy5tb2RlLnRocmVzaG9sZCxcbiAgICAgICAgICBzaG9ydGN1dDogcy5tb2RlLnNob3J0Y3V0LFxuICAgICAgICAgIGRlbGF5OiBzLm1vZGUuZGVsYXksXG4gICAgICAgIH0sXG4gICAgICB9KSk7XG4gIH1cblxuICAvKipcbiAgICogU2V0IGN1cnJlbnQgdm9pY2Ugc2V0dGluZ3MsIG92ZXJyaWRpbmcgdGhlIGN1cnJlbnQgc2V0dGluZ3MgdW50aWwgdGhpcyBzZXNzaW9uIGRpc2Nvbm5lY3RzLlxuICAgKiBUaGlzIGFsc28gbG9ja3MgdGhlIHNldHRpbmdzIGZvciBhbnkgb3RoZXIgcnBjIHNlc3Npb25zIHdoaWNoIG1heSBiZSBjb25uZWN0ZWQuXG4gICAqIEBwYXJhbSB7T2JqZWN0fSBhcmdzIFNldHRpbmdzXG4gICAqIEByZXR1cm5zIHtQcm9taXNlfVxuICAgKi9cbiAgc2V0Vm9pY2VTZXR0aW5ncyhhcmdzKSB7XG4gICAgcmV0dXJuIHRoaXMucmVxdWVzdChSUENDb21tYW5kcy5TRVRfVk9JQ0VfU0VUVElOR1MsIHtcbiAgICAgIGF1dG9tYXRpY19nYWluX2NvbnRyb2w6IGFyZ3MuYXV0b21hdGljR2FpbkNvbnRyb2wsXG4gICAgICBlY2hvX2NhbmNlbGxhdGlvbjogYXJncy5lY2hvQ2FuY2VsbGF0aW9uLFxuICAgICAgbm9pc2Vfc3VwcHJlc3Npb246IGFyZ3Mubm9pc2VTdXBwcmVzc2lvbixcbiAgICAgIHFvczogYXJncy5xb3MsXG4gICAgICBzaWxlbmNlX3dhcm5pbmc6IGFyZ3Muc2lsZW5jZVdhcm5pbmcsXG4gICAgICBkZWFmOiBhcmdzLmRlYWYsXG4gICAgICBtdXRlOiBhcmdzLm11dGUsXG4gICAgICBpbnB1dDogYXJncy5pbnB1dCA/IHtcbiAgICAgICAgZGV2aWNlX2lkOiBhcmdzLmlucHV0LmRldmljZSxcbiAgICAgICAgdm9sdW1lOiBhcmdzLmlucHV0LnZvbHVtZSxcbiAgICAgIH0gOiB1bmRlZmluZWQsXG4gICAgICBvdXRwdXQ6IGFyZ3Mub3V0cHV0ID8ge1xuICAgICAgICBkZXZpY2VfaWQ6IGFyZ3Mub3V0cHV0LmRldmljZSxcbiAgICAgICAgdm9sdW1lOiBhcmdzLm91dHB1dC52b2x1bWUsXG4gICAgICB9IDogdW5kZWZpbmVkLFxuICAgICAgbW9kZTogYXJncy5tb2RlID8ge1xuICAgICAgICBtb2RlOiBhcmdzLm1vZGUudHlwZSxcbiAgICAgICAgYXV0b190aHJlc2hvbGQ6IGFyZ3MubW9kZS5hdXRvVGhyZXNob2xkLFxuICAgICAgICB0aHJlc2hvbGQ6IGFyZ3MubW9kZS50aHJlc2hvbGQsXG4gICAgICAgIHNob3J0Y3V0OiBhcmdzLm1vZGUuc2hvcnRjdXQsXG4gICAgICAgIGRlbGF5OiBhcmdzLm1vZGUuZGVsYXksXG4gICAgICB9IDogdW5kZWZpbmVkLFxuICAgIH0pO1xuICB9XG5cbiAgLyoqXG4gICAqIENhcHR1cmUgYSBzaG9ydGN1dCB1c2luZyB0aGUgY2xpZW50XG4gICAqIFRoZSBjYWxsYmFjayB0YWtlcyAoa2V5LCBzdG9wKSB3aGVyZSBgc3RvcGAgaXMgYSBmdW5jdGlvbiB0aGF0IHdpbGwgc3RvcCBjYXB0dXJpbmcuXG4gICAqIFRoaXMgYHN0b3BgIGZ1bmN0aW9uIG11c3QgYmUgY2FsbGVkIGJlZm9yZSBkaXNjb25uZWN0aW5nIG9yIGVsc2UgdGhlIHVzZXIgd2lsbCBoYXZlXG4gICAqIHRvIHJlc3RhcnQgdGhlaXIgY2xpZW50LlxuICAgKiBAcGFyYW0ge0Z1bmN0aW9ufSBjYWxsYmFjayBDYWxsYmFjayBoYW5kbGluZyBrZXlzXG4gICAqIEByZXR1cm5zIHtQcm9taXNlPEZ1bmN0aW9uPn1cbiAgICovXG4gIGNhcHR1cmVTaG9ydGN1dChjYWxsYmFjaykge1xuICAgIGNvbnN0IHN1YmlkID0gc3ViS2V5KFJQQ0V2ZW50cy5DQVBUVVJFX1NIT1JUQ1VUX0NIQU5HRSk7XG4gICAgY29uc3Qgc3RvcCA9ICgpID0+IHtcbiAgICAgIHRoaXMuX3N1YnNjcmlwdGlvbnMuZGVsZXRlKHN1YmlkKTtcbiAgICAgIHJldHVybiB0aGlzLnJlcXVlc3QoUlBDQ29tbWFuZHMuQ0FQVFVSRV9TSE9SVENVVCwgeyBhY3Rpb246ICdTVE9QJyB9KTtcbiAgICB9O1xuICAgIHRoaXMuX3N1YnNjcmlwdGlvbnMuc2V0KHN1YmlkLCAoeyBzaG9ydGN1dCB9KSA9PiB7XG4gICAgICBjYWxsYmFjayhzaG9ydGN1dCwgc3RvcCk7XG4gICAgfSk7XG4gICAgcmV0dXJuIHRoaXMucmVxdWVzdChSUENDb21tYW5kcy5DQVBUVVJFX1NIT1JUQ1VULCB7IGFjdGlvbjogJ1NUQVJUJyB9KVxuICAgICAgLnRoZW4oKCkgPT4gc3RvcCk7XG4gIH1cblxuICAvKipcbiAgICogU2V0cyB0aGUgcHJlc2VuY2UgZm9yIHRoZSBsb2dnZWQgaW4gdXNlci5cbiAgICogQHBhcmFtIHtvYmplY3R9IGFyZ3MgVGhlIHJpY2ggcHJlc2VuY2UgdG8gcGFzcy5cbiAgICogQHBhcmFtIHtudW1iZXJ9IFtwaWRdIFRoZSBhcHBsaWNhdGlvbidzIHByb2Nlc3MgSUQuIERlZmF1bHRzIHRvIHRoZSBleGVjdXRpbmcgcHJvY2VzcycgUElELlxuICAgKiBAcmV0dXJucyB7UHJvbWlzZX1cbiAgICovXG4gIHNldEFjdGl2aXR5KGFyZ3MgPSB7fSwgcGlkID0gZ2V0UGlkKCkpIHtcbiAgICBsZXQgdGltZXN0YW1wcztcbiAgICBsZXQgYXNzZXRzO1xuICAgIGxldCBwYXJ0eTtcbiAgICBsZXQgc2VjcmV0cztcbiAgICBpZiAoYXJncy5zdGFydFRpbWVzdGFtcCB8fCBhcmdzLmVuZFRpbWVzdGFtcCkge1xuICAgICAgdGltZXN0YW1wcyA9IHtcbiAgICAgICAgc3RhcnQ6IGFyZ3Muc3RhcnRUaW1lc3RhbXAsXG4gICAgICAgIGVuZDogYXJncy5lbmRUaW1lc3RhbXAsXG4gICAgICB9O1xuICAgICAgaWYgKHRpbWVzdGFtcHMuc3RhcnQgaW5zdGFuY2VvZiBEYXRlKSB7XG4gICAgICAgIHRpbWVzdGFtcHMuc3RhcnQgPSBNYXRoLnJvdW5kKHRpbWVzdGFtcHMuc3RhcnQuZ2V0VGltZSgpKTtcbiAgICAgIH1cbiAgICAgIGlmICh0aW1lc3RhbXBzLmVuZCBpbnN0YW5jZW9mIERhdGUpIHtcbiAgICAgICAgdGltZXN0YW1wcy5lbmQgPSBNYXRoLnJvdW5kKHRpbWVzdGFtcHMuZW5kLmdldFRpbWUoKSk7XG4gICAgICB9XG4gICAgICBpZiAodGltZXN0YW1wcy5zdGFydCA+IDIxNDc0ODM2NDcwMDApIHtcbiAgICAgICAgdGhyb3cgbmV3IFJhbmdlRXJyb3IoJ3RpbWVzdGFtcHMuc3RhcnQgbXVzdCBmaXQgaW50byBhIHVuaXggdGltZXN0YW1wJyk7XG4gICAgICB9XG4gICAgICBpZiAodGltZXN0YW1wcy5lbmQgPiAyMTQ3NDgzNjQ3MDAwKSB7XG4gICAgICAgIHRocm93IG5ldyBSYW5nZUVycm9yKCd0aW1lc3RhbXBzLmVuZCBtdXN0IGZpdCBpbnRvIGEgdW5peCB0aW1lc3RhbXAnKTtcbiAgICAgIH1cbiAgICB9XG4gICAgaWYgKFxuICAgICAgYXJncy5sYXJnZUltYWdlS2V5IHx8IGFyZ3MubGFyZ2VJbWFnZVRleHRcbiAgICAgIHx8IGFyZ3Muc21hbGxJbWFnZUtleSB8fCBhcmdzLnNtYWxsSW1hZ2VUZXh0XG4gICAgKSB7XG4gICAgICBhc3NldHMgPSB7XG4gICAgICAgIGxhcmdlX2ltYWdlOiBhcmdzLmxhcmdlSW1hZ2VLZXksXG4gICAgICAgIGxhcmdlX3RleHQ6IGFyZ3MubGFyZ2VJbWFnZVRleHQsXG4gICAgICAgIHNtYWxsX2ltYWdlOiBhcmdzLnNtYWxsSW1hZ2VLZXksXG4gICAgICAgIHNtYWxsX3RleHQ6IGFyZ3Muc21hbGxJbWFnZVRleHQsXG4gICAgICB9O1xuICAgIH1cbiAgICBpZiAoYXJncy5wYXJ0eVNpemUgfHwgYXJncy5wYXJ0eUlkIHx8IGFyZ3MucGFydHlNYXgpIHtcbiAgICAgIHBhcnR5ID0geyBpZDogYXJncy5wYXJ0eUlkIH07XG4gICAgICBpZiAoYXJncy5wYXJ0eVNpemUgfHwgYXJncy5wYXJ0eU1heCkge1xuICAgICAgICBwYXJ0eS5zaXplID0gW2FyZ3MucGFydHlTaXplLCBhcmdzLnBhcnR5TWF4XTtcbiAgICAgIH1cbiAgICB9XG4gICAgaWYgKGFyZ3MubWF0Y2hTZWNyZXQgfHwgYXJncy5qb2luU2VjcmV0IHx8IGFyZ3Muc3BlY3RhdGVTZWNyZXQpIHtcbiAgICAgIHNlY3JldHMgPSB7XG4gICAgICAgIG1hdGNoOiBhcmdzLm1hdGNoU2VjcmV0LFxuICAgICAgICBqb2luOiBhcmdzLmpvaW5TZWNyZXQsXG4gICAgICAgIHNwZWN0YXRlOiBhcmdzLnNwZWN0YXRlU2VjcmV0LFxuICAgICAgfTtcbiAgICB9XG5cbiAgICByZXR1cm4gdGhpcy5yZXF1ZXN0KFJQQ0NvbW1hbmRzLlNFVF9BQ1RJVklUWSwge1xuICAgICAgcGlkLFxuICAgICAgYWN0aXZpdHk6IHtcbiAgICAgICAgc3RhdGU6IGFyZ3Muc3RhdGUsXG4gICAgICAgIGRldGFpbHM6IGFyZ3MuZGV0YWlscyxcbiAgICAgICAgdGltZXN0YW1wcyxcbiAgICAgICAgYXNzZXRzLFxuICAgICAgICBwYXJ0eSxcbiAgICAgICAgc2VjcmV0cyxcbiAgICAgICAgaW5zdGFuY2U6ICEhYXJncy5pbnN0YW5jZSxcbiAgICAgIH0sXG4gICAgfSk7XG4gIH1cblxuICAvKipcbiAgICogQ2xlYXJzIHRoZSBjdXJyZW50bHkgc2V0IHByZXNlbmNlLCBpZiBhbnkuIFRoaXMgd2lsbCBoaWRlIHRoZSBcIlBsYXlpbmcgWFwiIG1lc3NhZ2VcbiAgICogZGlzcGxheWVkIGJlbG93IHRoZSB1c2VyJ3MgbmFtZS5cbiAgICogQHBhcmFtIHtudW1iZXJ9IFtwaWRdIFRoZSBhcHBsaWNhdGlvbidzIHByb2Nlc3MgSUQuIERlZmF1bHRzIHRvIHRoZSBleGVjdXRpbmcgcHJvY2VzcycgUElELlxuICAgKiBAcmV0dXJucyB7UHJvbWlzZX1cbiAgICovXG4gIGNsZWFyQWN0aXZpdHkocGlkID0gZ2V0UGlkKCkpIHtcbiAgICByZXR1cm4gdGhpcy5yZXF1ZXN0KFJQQ0NvbW1hbmRzLlNFVF9BQ1RJVklUWSwge1xuICAgICAgcGlkLFxuICAgIH0pO1xuICB9XG5cbiAgLyoqXG4gICAqIEludml0ZSBhIHVzZXIgdG8gam9pbiB0aGUgZ2FtZSB0aGUgUlBDIHVzZXIgaXMgY3VycmVudGx5IHBsYXlpbmdcbiAgICogQHBhcmFtIHtVc2VyfSB1c2VyIFRoZSB1c2VyIHRvIGludml0ZVxuICAgKiBAcmV0dXJucyB7UHJvbWlzZX1cbiAgICovXG4gIHNlbmRKb2luSW52aXRlKHVzZXIpIHtcbiAgICByZXR1cm4gdGhpcy5yZXF1ZXN0KFJQQ0NvbW1hbmRzLlNFTkRfQUNUSVZJVFlfSk9JTl9JTlZJVEUsIHtcbiAgICAgIHVzZXJfaWQ6IHVzZXIuaWQgfHwgdXNlcixcbiAgICB9KTtcbiAgfVxuXG4gIC8qKlxuICAgKiBSZXF1ZXN0IHRvIGpvaW4gdGhlIGdhbWUgdGhlIHVzZXIgaXMgcGxheWluZ1xuICAgKiBAcGFyYW0ge1VzZXJ9IHVzZXIgVGhlIHVzZXIgd2hvc2UgZ2FtZSB5b3Ugd2FudCB0byByZXF1ZXN0IHRvIGpvaW5cbiAgICogQHJldHVybnMge1Byb21pc2V9XG4gICAqL1xuICBzZW5kSm9pblJlcXVlc3QodXNlcikge1xuICAgIHJldHVybiB0aGlzLnJlcXVlc3QoUlBDQ29tbWFuZHMuU0VORF9BQ1RJVklUWV9KT0lOX1JFUVVFU1QsIHtcbiAgICAgIHVzZXJfaWQ6IHVzZXIuaWQgfHwgdXNlcixcbiAgICB9KTtcbiAgfVxuXG4gIC8qKlxuICAgKiBSZWplY3QgYSBqb2luIHJlcXVlc3QgZnJvbSBhIHVzZXJcbiAgICogQHBhcmFtIHtVc2VyfSB1c2VyIFRoZSB1c2VyIHdob3NlIHJlcXVlc3QgeW91IHdpc2ggdG8gcmVqZWN0XG4gICAqIEByZXR1cm5zIHtQcm9taXNlfVxuICAgKi9cbiAgY2xvc2VKb2luUmVxdWVzdCh1c2VyKSB7XG4gICAgcmV0dXJuIHRoaXMucmVxdWVzdChSUENDb21tYW5kcy5DTE9TRV9BQ1RJVklUWV9KT0lOX1JFUVVFU1QsIHtcbiAgICAgIHVzZXJfaWQ6IHVzZXIuaWQgfHwgdXNlcixcbiAgICB9KTtcbiAgfVxuXG4gIGNyZWF0ZUxvYmJ5KHR5cGUsIGNhcGFjaXR5LCBtZXRhZGF0YSkge1xuICAgIHJldHVybiB0aGlzLnJlcXVlc3QoUlBDQ29tbWFuZHMuQ1JFQVRFX0xPQkJZLCB7XG4gICAgICB0eXBlLFxuICAgICAgY2FwYWNpdHksXG4gICAgICBtZXRhZGF0YSxcbiAgICB9KTtcbiAgfVxuXG4gIHVwZGF0ZUxvYmJ5KGxvYmJ5LCB7IHR5cGUsIG93bmVyLCBjYXBhY2l0eSwgbWV0YWRhdGEgfSA9IHt9KSB7XG4gICAgcmV0dXJuIHRoaXMucmVxdWVzdChSUENDb21tYW5kcy5VUERBVEVfTE9CQlksIHtcbiAgICAgIGlkOiBsb2JieS5pZCB8fCBsb2JieSxcbiAgICAgIHR5cGUsXG4gICAgICBvd25lcl9pZDogKG93bmVyICYmIG93bmVyLmlkKSB8fCBvd25lcixcbiAgICAgIGNhcGFjaXR5LFxuICAgICAgbWV0YWRhdGEsXG4gICAgfSk7XG4gIH1cblxuICBkZWxldGVMb2JieShsb2JieSkge1xuICAgIHJldHVybiB0aGlzLnJlcXVlc3QoUlBDQ29tbWFuZHMuREVMRVRFX0xPQkJZLCB7XG4gICAgICBpZDogbG9iYnkuaWQgfHwgbG9iYnksXG4gICAgfSk7XG4gIH1cblxuICBjb25uZWN0VG9Mb2JieShpZCwgc2VjcmV0KSB7XG4gICAgcmV0dXJuIHRoaXMucmVxdWVzdChSUENDb21tYW5kcy5DT05ORUNUX1RPX0xPQkJZLCB7XG4gICAgICBpZCxcbiAgICAgIHNlY3JldCxcbiAgICB9KTtcbiAgfVxuXG4gIHNlbmRUb0xvYmJ5KGxvYmJ5LCBkYXRhKSB7XG4gICAgcmV0dXJuIHRoaXMucmVxdWVzdChSUENDb21tYW5kcy5TRU5EX1RPX0xPQkJZLCB7XG4gICAgICBpZDogbG9iYnkuaWQgfHwgbG9iYnksXG4gICAgICBkYXRhLFxuICAgIH0pO1xuICB9XG5cbiAgZGlzY29ubmVjdEZyb21Mb2JieShsb2JieSkge1xuICAgIHJldHVybiB0aGlzLnJlcXVlc3QoUlBDQ29tbWFuZHMuRElTQ09OTkVDVF9GUk9NX0xPQkJZLCB7XG4gICAgICBpZDogbG9iYnkuaWQgfHwgbG9iYnksXG4gICAgfSk7XG4gIH1cblxuICB1cGRhdGVMb2JieU1lbWJlcihsb2JieSwgdXNlciwgbWV0YWRhdGEpIHtcbiAgICByZXR1cm4gdGhpcy5yZXF1ZXN0KFJQQ0NvbW1hbmRzLlVQREFURV9MT0JCWV9NRU1CRVIsIHtcbiAgICAgIGxvYmJ5X2lkOiBsb2JieS5pZCB8fCBsb2JieSxcbiAgICAgIHVzZXJfaWQ6IHVzZXIuaWQgfHwgdXNlcixcbiAgICAgIG1ldGFkYXRhLFxuICAgIH0pO1xuICB9XG5cbiAgZ2V0UmVsYXRpb25zaGlwcygpIHtcbiAgICBjb25zdCB0eXBlcyA9IE9iamVjdC5rZXlzKFJlbGF0aW9uc2hpcFR5cGVzKTtcbiAgICByZXR1cm4gdGhpcy5yZXF1ZXN0KFJQQ0NvbW1hbmRzLkdFVF9SRUxBVElPTlNISVBTKVxuICAgICAgLnRoZW4oKG8pID0+IG8ucmVsYXRpb25zaGlwcy5tYXAoKHIpID0+ICh7XG4gICAgICAgIC4uLnIsXG4gICAgICAgIHR5cGU6IHR5cGVzW3IudHlwZV0sXG4gICAgICB9KSkpO1xuICB9XG5cbiAgLyoqXG4gICAqIFN1YnNjcmliZSB0byBhbiBldmVudFxuICAgKiBAcGFyYW0ge3N0cmluZ30gZXZlbnQgTmFtZSBvZiBldmVudCBlLmcuIGBNRVNTQUdFX0NSRUFURWBcbiAgICogQHBhcmFtIHtPYmplY3R9IFthcmdzXSBBcmdzIGZvciBldmVudCBlLmcuIGB7IGNoYW5uZWxfaWQ6ICcxMjM0JyB9YFxuICAgKiBAcGFyYW0ge0Z1bmN0aW9ufSBjYWxsYmFjayBDYWxsYmFjayB3aGVuIGFuIGV2ZW50IGZvciB0aGUgc3Vic2NyaXB0aW9uIGlzIHRyaWdnZXJlZFxuICAgKiBAcmV0dXJucyB7UHJvbWlzZTxPYmplY3Q+fVxuICAgKi9cbiAgc3Vic2NyaWJlKGV2ZW50LCBhcmdzLCBjYWxsYmFjaykge1xuICAgIGlmICghY2FsbGJhY2sgJiYgdHlwZW9mIGFyZ3MgPT09ICdmdW5jdGlvbicpIHtcbiAgICAgIGNhbGxiYWNrID0gYXJncztcbiAgICAgIGFyZ3MgPSB1bmRlZmluZWQ7XG4gICAgfVxuICAgIHJldHVybiB0aGlzLnJlcXVlc3QoUlBDQ29tbWFuZHMuU1VCU0NSSUJFLCBhcmdzLCBldmVudCkudGhlbigoKSA9PiB7XG4gICAgICBjb25zdCBzdWJpZCA9IHN1YktleShldmVudCwgYXJncyk7XG4gICAgICB0aGlzLl9zdWJzY3JpcHRpb25zLnNldChzdWJpZCwgY2FsbGJhY2spO1xuICAgICAgcmV0dXJuIHtcbiAgICAgICAgdW5zdWJzY3JpYmU6ICgpID0+IHRoaXMucmVxdWVzdChSUENDb21tYW5kcy5VTlNVQlNDUklCRSwgYXJncywgZXZlbnQpXG4gICAgICAgICAgLnRoZW4oKCkgPT4gdGhpcy5fc3Vic2NyaXB0aW9ucy5kZWxldGUoc3ViaWQpKSxcbiAgICAgIH07XG4gICAgfSk7XG4gIH1cblxuICAvKipcbiAgICogRGVzdHJveSB0aGUgY2xpZW50XG4gICAqL1xuICBhc3luYyBkZXN0cm95KCkge1xuICAgIHRoaXMudHJhbnNwb3J0LmNsb3NlKCk7XG4gIH1cbn1cblxubW9kdWxlLmV4cG9ydHMgPSBSUENDbGllbnQ7XG4iLCIndXNlIHN0cmljdCc7XG5cbmNvbnN0IHV0aWwgPSByZXF1aXJlKCcuL3V0aWwnKTtcblxubW9kdWxlLmV4cG9ydHMgPSB7XG4gIENsaWVudDogcmVxdWlyZSgnLi9jbGllbnQnKSxcbiAgcmVnaXN0ZXIoaWQpIHtcbiAgICByZXR1cm4gdXRpbC5yZWdpc3RlcihgZGlzY29yZC0ke2lkfWApO1xuICB9LFxufTtcbiIsImltcG9ydCB7IE5vdGljZSB9IGZyb20gXCJvYnNpZGlhblwiO1xuaW1wb3J0IE9ic2lkaWFuRGlzY29yZFJQQyBmcm9tIFwiLi9tYWluXCI7XG5cbmV4cG9ydCBjbGFzcyBMb2dnZXIge1xuICBwbHVnaW46IE9ic2lkaWFuRGlzY29yZFJQQyA9ICh0aGlzIGFzIGFueSkucGx1Z2luO1xuXG4gIGxvZyhtZXNzYWdlOiBzdHJpbmcsIHNob3dQb3B1cHM6IGJvb2xlYW4pOiB2b2lkIHtcbiAgICBpZiAoc2hvd1BvcHVwcykge1xuICAgICAgbmV3IE5vdGljZShtZXNzYWdlKTtcbiAgICB9XG5cbiAgICBjb25zb2xlLmxvZyhgZGlzY29yZHJwYzogJHttZXNzYWdlfWApO1xuICB9XG5cbiAgbG9nSWdub3JlTm9Ob3RpY2UobWVzc2FnZTogc3RyaW5nKTogdm9pZCB7XG4gICAgbmV3IE5vdGljZShtZXNzYWdlKTtcbiAgICBjb25zb2xlLmxvZyhgZGlzY29yZHJwYzogJHttZXNzYWdlfWApO1xuICB9XG59XG4iLCJleHBvcnQgY2xhc3MgRGlzY29yZFJQQ1NldHRpbmdzIHtcbiAgc2hvd1ZhdWx0TmFtZTogYm9vbGVhbiA9IHRydWU7XG4gIHNob3dDdXJyZW50RmlsZU5hbWU6IGJvb2xlYW4gPSB0cnVlO1xuICBzaG93UG9wdXBzOiBib29sZWFuID0gdHJ1ZTtcbiAgY3VzdG9tVmF1bHROYW1lOiBzdHJpbmcgPSBcIlwiO1xuICBzaG93RmlsZUV4dGVuc2lvbjogYm9vbGVhbiA9IGZhbHNlO1xuICB1c2VMb2FkZWRUaW1lOiBib29sZWFuID0gZmFsc2U7XG59XG5cbmV4cG9ydCBlbnVtIFBsdWdpblN0YXRlIHtcbiAgY29ubmVjdGVkLFxuICBjb25uZWN0aW5nLFxuICBkaXNjb25uZWN0ZWQsXG59XG4iLCJpbXBvcnQgeyBQbHVnaW5TZXR0aW5nVGFiLCBTZXR0aW5nLCBURmlsZSB9IGZyb20gXCJvYnNpZGlhblwiO1xuaW1wb3J0IHsgTG9nZ2VyIH0gZnJvbSBcInNyYy9sb2dnZXJcIjtcbmltcG9ydCBPYnNpZGlhbkRpc2NvcmRSUEMgZnJvbSBcInNyYy9tYWluXCI7XG5cbmV4cG9ydCBjbGFzcyBEaXNjb3JkUlBDU2V0dGluZ3NUYWIgZXh0ZW5kcyBQbHVnaW5TZXR0aW5nVGFiIHtcbiAgcHVibGljIGxvZ2dlcjogTG9nZ2VyID0gbmV3IExvZ2dlcigpO1xuXG4gIGRpc3BsYXkoKTogdm9pZCB7XG4gICAgbGV0IHsgY29udGFpbmVyRWwgfSA9IHRoaXM7XG4gICAgY29uc3QgcGx1Z2luOiBPYnNpZGlhbkRpc2NvcmRSUEMgPSAodGhpcyBhcyBhbnkpLnBsdWdpbjtcblxuICAgIGNvbnRhaW5lckVsLmVtcHR5KCk7XG4gICAgY29udGFpbmVyRWwuY3JlYXRlRWwoXCJoMlwiLCB7IHRleHQ6IFwiRGlzY29yZCBSaWNoIFByZXNlbmNlIFNldHRpbmdzXCIgfSk7XG5cbiAgICBjb250YWluZXJFbC5jcmVhdGVFbChcImgzXCIsIHsgdGV4dDogXCJWYXVsdCBOYW1lIFNldHRpbmdzXCIgfSk7XG4gICAgbmV3IFNldHRpbmcoY29udGFpbmVyRWwpXG4gICAgICAuc2V0TmFtZShcIlNob3cgVmF1bHQgTmFtZVwiKVxuICAgICAgLnNldERlc2MoXG4gICAgICAgIFwiRW5hYmxlIHRoaXMgdG8gc2hvdyB0aGUgbmFtZSBvZiB0aGUgdmF1bHQgeW91IGFyZSB3b3JraW5nIHdpdGguXCJcbiAgICAgIClcbiAgICAgIC5hZGRUb2dnbGUoKGJvb2xlYW4pID0+XG4gICAgICAgIGJvb2xlYW4uc2V0VmFsdWUocGx1Z2luLnNldHRpbmdzLnNob3dWYXVsdE5hbWUpLm9uQ2hhbmdlKCh2YWx1ZSkgPT4ge1xuICAgICAgICAgIHBsdWdpbi5zZXR0aW5ncy5zaG93VmF1bHROYW1lID0gdmFsdWU7XG4gICAgICAgICAgcGx1Z2luLnNhdmVEYXRhKHBsdWdpbi5zZXR0aW5ncyk7XG5cbiAgICAgICAgICBpZiAoYm9vbGVhbi5nZXRWYWx1ZSgpKSB7XG4gICAgICAgICAgICB0aGlzLmxvZ2dlci5sb2dJZ25vcmVOb05vdGljZShcIlZhdWx0IE5hbWUgaXMgbm93IFZpc2FibGVcIik7XG4gICAgICAgICAgfSBlbHNlIHtcbiAgICAgICAgICAgIHRoaXMubG9nZ2VyLmxvZ0lnbm9yZU5vTm90aWNlKFwiVmF1bHQgTmFtZSBpcyBubyBsb25nZXIgVmlzYWJsZVwiKTtcbiAgICAgICAgICB9XG5cbiAgICAgICAgICBwbHVnaW4uc2V0QWN0aXZpdHkoXG4gICAgICAgICAgICB0aGlzLmFwcC52YXVsdC5nZXROYW1lKCksXG4gICAgICAgICAgICBwbHVnaW4uY3VycmVudEZpbGUuYmFzZW5hbWUsXG4gICAgICAgICAgICBwbHVnaW4uY3VycmVudEZpbGUuZXh0ZW5zaW9uXG4gICAgICAgICAgKTtcbiAgICAgICAgfSlcbiAgICAgICk7XG5cbiAgICBuZXcgU2V0dGluZyhjb250YWluZXJFbClcbiAgICAgIC5zZXROYW1lKFwiU2V0IEN1c3RvbSBWYXVsdCBOYW1lXCIpXG4gICAgICAuc2V0RGVzYyhcbiAgICAgICAgXCJDaGFuZ2UgdGhlIHZhdWx0IG5hbWUgc2hvd24gcHVibGljbHkuIExlYXZlIGJsYW5rIHRvIHVzZSB5b3VyIGFjdHVhbCB2YXVsdCBuYW1lLlwiXG4gICAgICApXG4gICAgICAuYWRkVGV4dCgodGV4dCkgPT5cbiAgICAgICAgdGV4dC5zZXRWYWx1ZShwbHVnaW4uc2V0dGluZ3MuY3VzdG9tVmF1bHROYW1lKS5vbkNoYW5nZSgodmFsdWUpID0+IHtcbiAgICAgICAgICBwbHVnaW4uc2V0dGluZ3MuY3VzdG9tVmF1bHROYW1lID0gdmFsdWU7XG4gICAgICAgICAgcGx1Z2luLnNhdmVEYXRhKHBsdWdpbi5zZXR0aW5ncyk7XG5cbiAgICAgICAgICBwbHVnaW4uc2V0QWN0aXZpdHkoXG4gICAgICAgICAgICB0aGlzLmFwcC52YXVsdC5nZXROYW1lKCksXG4gICAgICAgICAgICBwbHVnaW4uY3VycmVudEZpbGUuYmFzZW5hbWUsXG4gICAgICAgICAgICBwbHVnaW4uY3VycmVudEZpbGUuZXh0ZW5zaW9uXG4gICAgICAgICAgKTtcbiAgICAgICAgfSlcbiAgICAgICk7XG5cbiAgICBjb250YWluZXJFbC5jcmVhdGVFbChcImgzXCIsIHsgdGV4dDogXCJGaWxlIE5hbWUgU2V0dGluZ3NcIiB9KTtcbiAgICBuZXcgU2V0dGluZyhjb250YWluZXJFbClcbiAgICAgIC5zZXROYW1lKFwiU2hvdyBDdXJyZW50IEZpbGUgTmFtZVwiKVxuICAgICAgLnNldERlc2MoXCJFbmFibGUgdGhpcyB0byBzaG93IHRoZSBuYW1lIG9mIHRoZSBmaWxlIHlvdSBhcmUgd29ya2luZyBvbi5cIilcbiAgICAgIC5hZGRUb2dnbGUoKGJvb2xlYW4pID0+XG4gICAgICAgIGJvb2xlYW5cbiAgICAgICAgICAuc2V0VmFsdWUocGx1Z2luLnNldHRpbmdzLnNob3dDdXJyZW50RmlsZU5hbWUpXG4gICAgICAgICAgLm9uQ2hhbmdlKCh2YWx1ZSkgPT4ge1xuICAgICAgICAgICAgcGx1Z2luLnNldHRpbmdzLnNob3dDdXJyZW50RmlsZU5hbWUgPSB2YWx1ZTtcbiAgICAgICAgICAgIHBsdWdpbi5zYXZlRGF0YShwbHVnaW4uc2V0dGluZ3MpO1xuXG4gICAgICAgICAgICBpZiAoYm9vbGVhbi5nZXRWYWx1ZSgpKSB7XG4gICAgICAgICAgICAgIHRoaXMubG9nZ2VyLmxvZ0lnbm9yZU5vTm90aWNlKFwiRmlsZSBOYW1lIGlzIG5vdyBWaXNhYmxlXCIpO1xuICAgICAgICAgICAgfSBlbHNlIHtcbiAgICAgICAgICAgICAgdGhpcy5sb2dnZXIubG9nSWdub3JlTm9Ob3RpY2UoXCJGaWxlIE5hbWUgaXMgbm8gbG9uZ2VyIFZpc2FibGVcIik7XG4gICAgICAgICAgICB9XG5cbiAgICAgICAgICAgIHBsdWdpbi5zZXRBY3Rpdml0eShcbiAgICAgICAgICAgICAgdGhpcy5hcHAudmF1bHQuZ2V0TmFtZSgpLFxuICAgICAgICAgICAgICBwbHVnaW4uY3VycmVudEZpbGUuYmFzZW5hbWUsXG4gICAgICAgICAgICAgIHBsdWdpbi5jdXJyZW50RmlsZS5leHRlbnNpb25cbiAgICAgICAgICAgICk7XG4gICAgICAgICAgfSlcbiAgICAgICk7XG5cbiAgICBuZXcgU2V0dGluZyhjb250YWluZXJFbClcbiAgICAgIC5zZXROYW1lKFwiU2hvdyBGaWxlIEV4dGVuc2lvblwiKVxuICAgICAgLnNldERlc2MoXCJFbmFibGUgdGhpcyB0byBzaG93IGZpbGUgZXh0ZW5zaW9uLlwiKVxuICAgICAgLmFkZFRvZ2dsZSgoYm9vbGVhbikgPT5cbiAgICAgICAgYm9vbGVhblxuICAgICAgICAgIC5zZXRWYWx1ZShwbHVnaW4uc2V0dGluZ3Muc2hvd0ZpbGVFeHRlbnNpb24pXG4gICAgICAgICAgLm9uQ2hhbmdlKCh2YWx1ZSkgPT4ge1xuICAgICAgICAgICAgcGx1Z2luLnNldHRpbmdzLnNob3dGaWxlRXh0ZW5zaW9uID0gdmFsdWU7XG4gICAgICAgICAgICBwbHVnaW4uc2F2ZURhdGEocGx1Z2luLnNldHRpbmdzKTtcblxuICAgICAgICAgICAgcGx1Z2luLnNldEFjdGl2aXR5KFxuICAgICAgICAgICAgICB0aGlzLmFwcC52YXVsdC5nZXROYW1lKCksXG4gICAgICAgICAgICAgIHBsdWdpbi5jdXJyZW50RmlsZS5iYXNlbmFtZSxcbiAgICAgICAgICAgICAgcGx1Z2luLmN1cnJlbnRGaWxlLmV4dGVuc2lvblxuICAgICAgICAgICAgKTtcbiAgICAgICAgICB9KVxuICAgICAgKTtcblxuICAgIGNvbnRhaW5lckVsLmNyZWF0ZUVsKFwiaDNcIiwgeyB0ZXh0OiBcIlRpbWUgU2V0dGluZ3NcIiB9KTtcbiAgICBuZXcgU2V0dGluZyhjb250YWluZXJFbClcbiAgICAgIC5zZXROYW1lKFwiVXNlIE9ic2lkaWFuIFRvdGFsIFRpbWVcIilcbiAgICAgIC5zZXREZXNjKFxuICAgICAgICBcIkVuYWJsZSB0byB1c2UgdGhlIHRvdGFsIHRpbWUgeW91IGhhdmUgYmVlbiB1c2luZyBPYnNpZGlhbiwgaW5zdGVhZCBvZiB0aGUgdGltZSBzcGVudCBlZGl0aW5nIGEgc2luZ2xlIGZpbGUuXCJcbiAgICAgIClcbiAgICAgIC5hZGRUb2dnbGUoKGJvb2xlYW4pID0+IHtcbiAgICAgICAgYm9vbGVhbi5zZXRWYWx1ZShwbHVnaW4uc2V0dGluZ3MudXNlTG9hZGVkVGltZSkub25DaGFuZ2UoKHZhbHVlKSA9PiB7XG4gICAgICAgICAgcGx1Z2luLnNldHRpbmdzLnVzZUxvYWRlZFRpbWUgPSB2YWx1ZTtcbiAgICAgICAgICBwbHVnaW4uc2F2ZURhdGEocGx1Z2luLnNldHRpbmdzKTtcblxuICAgICAgICAgIHBsdWdpbi5zZXRBY3Rpdml0eShcbiAgICAgICAgICAgIHRoaXMuYXBwLnZhdWx0LmdldE5hbWUoKSxcbiAgICAgICAgICAgIHBsdWdpbi5jdXJyZW50RmlsZS5iYXNlbmFtZSxcbiAgICAgICAgICAgIHBsdWdpbi5jdXJyZW50RmlsZS5leHRlbnNpb25cbiAgICAgICAgICApO1xuICAgICAgICB9KTtcbiAgICAgIH0pO1xuXG4gICAgY29udGFpbmVyRWwuY3JlYXRlRWwoXCJoM1wiLCB7IHRleHQ6IFwiTm90aWNlIFNldHRpbmdzXCIgfSk7XG4gICAgbmV3IFNldHRpbmcoY29udGFpbmVyRWwpXG4gICAgICAuc2V0TmFtZShcIlNob3cgTm90aWNlc1wiKVxuICAgICAgLnNldERlc2MoXCJFbmFibGUgdGhpcyB0byBzaG93IGNvbm5lY3Rpb24gTm90aWNlcy5cIilcbiAgICAgIC5hZGRUb2dnbGUoKGJvb2xlYW4pID0+XG4gICAgICAgIGJvb2xlYW4uc2V0VmFsdWUocGx1Z2luLnNldHRpbmdzLnNob3dQb3B1cHMpLm9uQ2hhbmdlKCh2YWx1ZSkgPT4ge1xuICAgICAgICAgIHBsdWdpbi5zZXR0aW5ncy5zaG93UG9wdXBzID0gdmFsdWU7XG4gICAgICAgICAgcGx1Z2luLnNhdmVEYXRhKHBsdWdpbi5zZXR0aW5ncyk7XG5cbiAgICAgICAgICBpZiAoYm9vbGVhbi5nZXRWYWx1ZSgpKSB7XG4gICAgICAgICAgICB0aGlzLmxvZ2dlci5sb2dJZ25vcmVOb05vdGljZShcIk5vdGljZXMgRW5hYmxlZFwiKTtcbiAgICAgICAgICB9IGVsc2Uge1xuICAgICAgICAgICAgdGhpcy5sb2dnZXIubG9nSWdub3JlTm9Ob3RpY2UoXCJOb3RpY2VzIERpc2FibGVkXCIpO1xuICAgICAgICAgIH1cblxuICAgICAgICAgIHBsdWdpbi5zZXRBY3Rpdml0eShcbiAgICAgICAgICAgIHRoaXMuYXBwLnZhdWx0LmdldE5hbWUoKSxcbiAgICAgICAgICAgIHBsdWdpbi5jdXJyZW50RmlsZS5iYXNlbmFtZSxcbiAgICAgICAgICAgIHBsdWdpbi5jdXJyZW50RmlsZS5leHRlbnNpb25cbiAgICAgICAgICApO1xuICAgICAgICB9KVxuICAgICAgKTtcbiAgfVxufVxuIiwiaW1wb3J0IHsgUGx1Z2luU3RhdGUgfSBmcm9tIFwiLi9zZXR0aW5ncy9zZXR0aW5nc1wiO1xuXG5leHBvcnQgY2xhc3MgU3RhdHVzQmFyIHtcbiAgcHJpdmF0ZSBzdGF0dXNCYXJFbDogSFRNTEVsZW1lbnQ7XG5cbiAgY29uc3RydWN0b3Ioc3RhdHVzQmFyRWw6IEhUTUxFbGVtZW50KSB7XG4gICAgdGhpcy5zdGF0dXNCYXJFbCA9IHN0YXR1c0JhckVsO1xuICB9XG5cbiAgZGlzcGxheVN0YXRlKHN0YXRlOiBQbHVnaW5TdGF0ZSkge1xuICAgIHN3aXRjaCAoc3RhdGUpIHtcbiAgICAgIGNhc2UgUGx1Z2luU3RhdGUuY29ubmVjdGVkOlxuICAgICAgICB0aGlzLmRpc3BsYXlDb25uZWN0ZWQoMjAwMCk7XG4gICAgICAgIGJyZWFrO1xuICAgICAgY2FzZSBQbHVnaW5TdGF0ZS5jb25uZWN0aW5nOlxuICAgICAgICB0aGlzLnN0YXR1c0JhckVsLnNldFRleHQoYENvbm5lY3RpbmcgdG8gRGlzY29yZC4uLmApO1xuICAgICAgICBicmVhaztcbiAgICAgIGNhc2UgUGx1Z2luU3RhdGUuZGlzY29ubmVjdGVkOlxuICAgICAgICB0aGlzLnN0YXR1c0JhckVsLnNldFRleHQoYFxcdXsxRjVEOH0gUmVjb25uZWN0IHRvIERpc2NvcmRgKTtcbiAgICAgICAgYnJlYWs7XG4gICAgfVxuICB9XG5cbiAgZGlzcGxheUNvbm5lY3RlZCh0aW1lb3V0OiBudW1iZXIpIHtcbiAgICB0aGlzLnN0YXR1c0JhckVsLnNldFRleHQoYFxcdXsxRjMwRH0gQ29ubmVjdGVkIHRvIERpc2NvcmRgKTtcblxuICAgIGlmICh0aW1lb3V0ICYmIHRpbWVvdXQgPiAwKSB7XG4gICAgICB3aW5kb3cuc2V0VGltZW91dCgoKSA9PiB7XG4gICAgICAgIHRoaXMuc3RhdHVzQmFyRWwuc2V0VGV4dChgXFx1ezFGMzBEfWApO1xuICAgICAgfSwgdGltZW91dCk7XG4gICAgfVxuICB9XG59XG4iLCJpbXBvcnQgeyBDbGllbnQgfSBmcm9tIFwiZGlzY29yZC1ycGNcIjtcclxuaW1wb3J0IHsgUGx1Z2luLCBQbHVnaW5NYW5pZmVzdCwgVEZpbGUgfSBmcm9tIFwib2JzaWRpYW5cIjtcclxuaW1wb3J0IHsgTG9nZ2VyIH0gZnJvbSBcIi4vbG9nZ2VyXCI7XHJcbmltcG9ydCB7IERpc2NvcmRSUENTZXR0aW5ncywgUGx1Z2luU3RhdGUgfSBmcm9tIFwiLi9zZXR0aW5ncy9zZXR0aW5nc1wiO1xyXG5pbXBvcnQgeyBEaXNjb3JkUlBDU2V0dGluZ3NUYWIgfSBmcm9tIFwiLi9zZXR0aW5ncy9zZXR0aW5ncy10YWJcIjtcclxuaW1wb3J0IHsgU3RhdHVzQmFyIH0gZnJvbSBcIi4vc3RhdHVzLWJhclwiO1xyXG5cclxuZXhwb3J0IGRlZmF1bHQgY2xhc3MgT2JzaWRpYW5EaXNjb3JkUlBDIGV4dGVuZHMgUGx1Z2luIHtcclxuICBwdWJsaWMgc3RhdGU6IFBsdWdpblN0YXRlO1xyXG4gIHB1YmxpYyBzZXR0aW5nczogRGlzY29yZFJQQ1NldHRpbmdzO1xyXG4gIHB1YmxpYyBzdGF0dXNCYXI6IFN0YXR1c0JhcjtcclxuICBwdWJsaWMgcnBjOiBDbGllbnQ7XHJcbiAgcHVibGljIGxvZ2dlcjogTG9nZ2VyID0gbmV3IExvZ2dlcigpO1xyXG4gIHB1YmxpYyBjdXJyZW50RmlsZTogVEZpbGU7XHJcbiAgcHVibGljIGxvYWRlZFRpbWU6IERhdGU7XHJcblxyXG4gIHNldFN0YXRlKHN0YXRlOiBQbHVnaW5TdGF0ZSkge1xyXG4gICAgdGhpcy5zdGF0ZSA9IHN0YXRlO1xyXG4gIH1cclxuXHJcbiAgZ2V0U3RhdGUoKTogUGx1Z2luU3RhdGUge1xyXG4gICAgcmV0dXJuIHRoaXMuc3RhdGU7XHJcbiAgfVxyXG5cclxuICBwdWJsaWMgZ2V0QXBwKCk6IGFueSB7XHJcbiAgICByZXR1cm4gdGhpcy5hcHA7XHJcbiAgfVxyXG5cclxuICBwdWJsaWMgZ2V0UGx1Z2luTWFuaWZlc3QoKTogUGx1Z2luTWFuaWZlc3Qge1xyXG4gICAgcmV0dXJuIHRoaXMubWFuaWZlc3Q7XHJcbiAgfVxyXG5cclxuICBhc3luYyBvbmxvYWQoKSB7XHJcbiAgICB0aGlzLmxvYWRlZFRpbWUgPSBuZXcgRGF0ZSgpO1xyXG4gICAgbGV0IHN0YXR1c0JhckVsID0gdGhpcy5hZGRTdGF0dXNCYXJJdGVtKCk7XHJcbiAgICB0aGlzLnN0YXR1c0JhciA9IG5ldyBTdGF0dXNCYXIoc3RhdHVzQmFyRWwpO1xyXG5cclxuICAgIHRoaXMuc2V0dGluZ3MgPSAoYXdhaXQgdGhpcy5sb2FkRGF0YSgpKSB8fCBuZXcgRGlzY29yZFJQQ1NldHRpbmdzKCk7XHJcblxyXG4gICAgdGhpcy5yZWdpc3RlckV2ZW50KFxyXG4gICAgICB0aGlzLmFwcC53b3Jrc3BhY2Uub24oXCJmaWxlLW9wZW5cIiwgdGhpcy5vbkZpbGVPcGVuLCB0aGlzKVxyXG4gICAgKTtcclxuXHJcbiAgICB0aGlzLnJlZ2lzdGVyRG9tRXZlbnQoc3RhdHVzQmFyRWwsIFwiY2xpY2tcIiwgYXN5bmMgKCkgPT4ge1xyXG4gICAgICBpZiAodGhpcy5nZXRTdGF0ZSgpID09IFBsdWdpblN0YXRlLmRpc2Nvbm5lY3RlZCkge1xyXG4gICAgICAgIGF3YWl0IHRoaXMuY29ubmVjdERpc2NvcmQoKTtcclxuICAgICAgfVxyXG4gICAgfSk7XHJcblxyXG4gICAgdGhpcy5hZGRTZXR0aW5nVGFiKG5ldyBEaXNjb3JkUlBDU2V0dGluZ3NUYWIodGhpcy5hcHAsIHRoaXMpKTtcclxuXHJcbiAgICB0aGlzLmFkZENvbW1hbmQoe1xyXG4gICAgICBpZDogXCJyZWNvbm5lY3QtZGlzY29yZFwiLFxyXG4gICAgICBuYW1lOiBcIlJlY29ubmVjdCB0byBEaXNjb3JkXCIsXHJcbiAgICAgIGNhbGxiYWNrOiBhc3luYyAoKSA9PiBhd2FpdCB0aGlzLmNvbm5lY3REaXNjb3JkKCksXHJcbiAgICB9KTtcclxuXHJcbiAgICBhd2FpdCB0aGlzLmNvbm5lY3REaXNjb3JkKCk7XHJcblxyXG4gICAgbGV0IGFjdGl2ZUxlYWYgPSB0aGlzLmFwcC53b3Jrc3BhY2UuYWN0aXZlTGVhZjtcclxuICAgIGxldCBmaWxlczogVEZpbGVbXSA9IHRoaXMuYXBwLnZhdWx0LmdldE1hcmtkb3duRmlsZXMoKTtcclxuXHJcbiAgICBmaWxlcy5mb3JFYWNoKChmaWxlKSA9PiB7XHJcbiAgICAgIGlmIChmaWxlLmJhc2VuYW1lID09PSBhY3RpdmVMZWFmLmdldERpc3BsYXlUZXh0KCkpIHtcclxuICAgICAgICB0aGlzLm9uRmlsZU9wZW4oZmlsZSk7XHJcbiAgICAgIH1cclxuICAgIH0pO1xyXG4gIH1cclxuXHJcbiAgYXN5bmMgb25GaWxlT3BlbihmaWxlOiBURmlsZSkge1xyXG4gICAgdGhpcy5jdXJyZW50RmlsZSA9IGZpbGU7XHJcbiAgICBpZiAodGhpcy5nZXRTdGF0ZSgpID09PSBQbHVnaW5TdGF0ZS5jb25uZWN0ZWQpIHtcclxuICAgICAgYXdhaXQgdGhpcy5zZXRBY3Rpdml0eShcclxuICAgICAgICB0aGlzLmFwcC52YXVsdC5nZXROYW1lKCksXHJcbiAgICAgICAgZmlsZS5iYXNlbmFtZSxcclxuICAgICAgICBmaWxlLmV4dGVuc2lvblxyXG4gICAgICApO1xyXG4gICAgfVxyXG4gIH1cclxuXHJcbiAgYXN5bmMgb251bmxvYWQoKSB7XHJcbiAgICBhd2FpdCB0aGlzLnNhdmVEYXRhKHRoaXMuc2V0dGluZ3MpO1xyXG4gICAgdGhpcy5ycGMuY2xlYXJBY3Rpdml0eSgpO1xyXG4gICAgdGhpcy5ycGMuZGVzdHJveSgpO1xyXG4gIH1cclxuXHJcbiAgYXN5bmMgY29ubmVjdERpc2NvcmQoKTogUHJvbWlzZTx2b2lkPiB7XHJcbiAgICB0aGlzLnJwYyA9IG5ldyBDbGllbnQoe1xyXG4gICAgICB0cmFuc3BvcnQ6IFwiaXBjXCIsXHJcbiAgICB9KTtcclxuXHJcbiAgICB0aGlzLnNldFN0YXRlKFBsdWdpblN0YXRlLmNvbm5lY3RpbmcpO1xyXG4gICAgdGhpcy5zdGF0dXNCYXIuZGlzcGxheVN0YXRlKHRoaXMuZ2V0U3RhdGUoKSk7XHJcblxyXG4gICAgdGhpcy5ycGMub25jZShcInJlYWR5XCIsICgpID0+IHtcclxuICAgICAgdGhpcy5zZXRTdGF0ZShQbHVnaW5TdGF0ZS5jb25uZWN0ZWQpO1xyXG4gICAgICB0aGlzLnN0YXR1c0Jhci5kaXNwbGF5U3RhdGUodGhpcy5nZXRTdGF0ZSgpKTtcclxuICAgICAgdGhpcy5sb2dnZXIubG9nKFwiQ29ubmVjdGVkIHRvIERpc2NvcmRcIiwgdGhpcy5zZXR0aW5ncy5zaG93UG9wdXBzKTtcclxuICAgIH0pO1xyXG5cclxuICAgIHRyeSB7XHJcbiAgICAgIGF3YWl0IHRoaXMucnBjLmxvZ2luKHtcclxuICAgICAgICBjbGllbnRJZDogXCI3NjM4MTMxODUwMjIxOTc4MzFcIixcclxuICAgICAgfSk7XHJcbiAgICAgIGF3YWl0IHRoaXMuc2V0QWN0aXZpdHkodGhpcy5hcHAudmF1bHQuZ2V0TmFtZSgpLCBcIi4uLlwiLCBcIlwiKTtcclxuICAgIH0gY2F0Y2ggKGVycm9yKSB7XHJcbiAgICAgIHRoaXMuc2V0U3RhdGUoUGx1Z2luU3RhdGUuZGlzY29ubmVjdGVkKTtcclxuICAgICAgdGhpcy5zdGF0dXNCYXIuZGlzcGxheVN0YXRlKHRoaXMuZ2V0U3RhdGUoKSk7XHJcbiAgICAgIHRoaXMubG9nZ2VyLmxvZyhcIkZhaWxlZCB0byBjb25uZWN0IHRvIERpc2NvcmRcIiwgdGhpcy5zZXR0aW5ncy5zaG93UG9wdXBzKTtcclxuICAgIH1cclxuICB9XHJcblxyXG4gIGFzeW5jIHNldEFjdGl2aXR5KFxyXG4gICAgdmF1bHROYW1lOiBzdHJpbmcsXHJcbiAgICBmaWxlTmFtZTogc3RyaW5nLFxyXG4gICAgZmlsZUV4dGVuc2lvbjogc3RyaW5nXHJcbiAgKTogUHJvbWlzZTx2b2lkPiB7XHJcbiAgICBpZiAodGhpcy5nZXRTdGF0ZSgpID09PSBQbHVnaW5TdGF0ZS5jb25uZWN0ZWQpIHtcclxuICAgICAgbGV0IHZhdWx0OiBzdHJpbmc7XHJcbiAgICAgIGlmICh0aGlzLnNldHRpbmdzLmN1c3RvbVZhdWx0TmFtZSA9PT0gXCJcIikge1xyXG4gICAgICAgIHZhdWx0ID0gdmF1bHROYW1lO1xyXG4gICAgICB9IGVsc2Uge1xyXG4gICAgICAgIHZhdWx0ID0gdGhpcy5zZXR0aW5ncy5jdXN0b21WYXVsdE5hbWU7XHJcbiAgICAgIH1cclxuXHJcbiAgICAgIGxldCBmaWxlOiBzdHJpbmc7XHJcbiAgICAgIGlmICh0aGlzLnNldHRpbmdzLnNob3dGaWxlRXh0ZW5zaW9uKSB7XHJcbiAgICAgICAgZmlsZSA9IGZpbGVOYW1lICsgXCIuXCIgKyBmaWxlRXh0ZW5zaW9uO1xyXG4gICAgICB9IGVsc2Uge1xyXG4gICAgICAgIGZpbGUgPSBmaWxlTmFtZTtcclxuICAgICAgfVxyXG5cclxuICAgICAgbGV0IGRhdGU6IERhdGU7XHJcbiAgICAgIGlmICh0aGlzLnNldHRpbmdzLnVzZUxvYWRlZFRpbWUpIHtcclxuICAgICAgICBkYXRlID0gdGhpcy5sb2FkZWRUaW1lO1xyXG4gICAgICB9IGVsc2Uge1xyXG4gICAgICAgIGRhdGUgPSBuZXcgRGF0ZSgpO1xyXG4gICAgICB9XHJcblxyXG4gICAgICBpZiAodGhpcy5zZXR0aW5ncy5zaG93VmF1bHROYW1lICYmIHRoaXMuc2V0dGluZ3Muc2hvd0N1cnJlbnRGaWxlTmFtZSkge1xyXG4gICAgICAgIGF3YWl0IHRoaXMucnBjLnNldEFjdGl2aXR5KHtcclxuICAgICAgICAgIGRldGFpbHM6IGBFZGl0aW5nICR7ZmlsZX1gLFxyXG4gICAgICAgICAgc3RhdGU6IGBWYXVsdDogJHt2YXVsdH1gLFxyXG4gICAgICAgICAgc3RhcnRUaW1lc3RhbXA6IGRhdGUsXHJcbiAgICAgICAgICBsYXJnZUltYWdlS2V5OiBcImxvZ29cIixcclxuICAgICAgICAgIGxhcmdlSW1hZ2VUZXh0OiBcIk9ic2lkaWFuXCIsXHJcbiAgICAgICAgfSk7XHJcbiAgICAgIH0gZWxzZSBpZiAodGhpcy5zZXR0aW5ncy5zaG93VmF1bHROYW1lKSB7XHJcbiAgICAgICAgYXdhaXQgdGhpcy5ycGMuc2V0QWN0aXZpdHkoe1xyXG4gICAgICAgICAgc3RhdGU6IGBWYXVsdDogJHt2YXVsdH1gLFxyXG4gICAgICAgICAgc3RhcnRUaW1lc3RhbXA6IGRhdGUsXHJcbiAgICAgICAgICBsYXJnZUltYWdlS2V5OiBcImxvZ29cIixcclxuICAgICAgICAgIGxhcmdlSW1hZ2VUZXh0OiBcIk9ic2lkaWFuXCIsXHJcbiAgICAgICAgfSk7XHJcbiAgICAgIH0gZWxzZSBpZiAodGhpcy5zZXR0aW5ncy5zaG93Q3VycmVudEZpbGVOYW1lKSB7XHJcbiAgICAgICAgYXdhaXQgdGhpcy5ycGMuc2V0QWN0aXZpdHkoe1xyXG4gICAgICAgICAgZGV0YWlsczogYEVkaXRpbmcgJHtmaWxlfWAsXHJcbiAgICAgICAgICBzdGFydFRpbWVzdGFtcDogZGF0ZSxcclxuICAgICAgICAgIGxhcmdlSW1hZ2VLZXk6IFwibG9nb1wiLFxyXG4gICAgICAgICAgbGFyZ2VJbWFnZVRleHQ6IFwiT2JzaWRpYW5cIixcclxuICAgICAgICB9KTtcclxuICAgICAgfSBlbHNlIHtcclxuICAgICAgICBhd2FpdCB0aGlzLnJwYy5zZXRBY3Rpdml0eSh7XHJcbiAgICAgICAgICBzdGFydFRpbWVzdGFtcDogbmV3IERhdGUoKSxcclxuICAgICAgICAgIGxhcmdlSW1hZ2VLZXk6IFwibG9nb1wiLFxyXG4gICAgICAgICAgbGFyZ2VJbWFnZVRleHQ6IFwiT2JzaWRpYW5cIixcclxuICAgICAgICB9KTtcclxuICAgICAgfVxyXG4gICAgfVxyXG4gIH1cclxufVxyXG4iXSwibmFtZXMiOlsicmVxdWlyZSQkMCIsIm5ldCIsImZldGNoIiwiRXZlbnRFbWl0dGVyIiwiYnJvd3NlciIsInJlcXVpcmUkJDEiLCJzZXRUaW1lb3V0IiwiUlBDQ29tbWFuZHMiLCJSUENFdmVudHMiLCJSZWxhdGlvbnNoaXBUeXBlcyIsInV1aWQiLCJyZXF1aXJlJCQyIiwiTm90aWNlIiwiU2V0dGluZyIsIlBsdWdpblNldHRpbmdUYWIiLCJDbGllbnQiLCJQbHVnaW4iXSwibWFwcGluZ3MiOiI7Ozs7Ozs7Ozs7Ozs7OztBQUFBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0EsSUFBSSxhQUFhLEdBQUcsU0FBUyxDQUFDLEVBQUUsQ0FBQyxFQUFFO0FBQ25DLElBQUksYUFBYSxHQUFHLE1BQU0sQ0FBQyxjQUFjO0FBQ3pDLFNBQVMsRUFBRSxTQUFTLEVBQUUsRUFBRSxFQUFFLFlBQVksS0FBSyxJQUFJLFVBQVUsQ0FBQyxFQUFFLENBQUMsRUFBRSxFQUFFLENBQUMsQ0FBQyxTQUFTLEdBQUcsQ0FBQyxDQUFDLEVBQUUsQ0FBQztBQUNwRixRQUFRLFVBQVUsQ0FBQyxFQUFFLENBQUMsRUFBRSxFQUFFLEtBQUssSUFBSSxDQUFDLElBQUksQ0FBQyxFQUFFLElBQUksTUFBTSxDQUFDLFNBQVMsQ0FBQyxjQUFjLENBQUMsSUFBSSxDQUFDLENBQUMsRUFBRSxDQUFDLENBQUMsRUFBRSxDQUFDLENBQUMsQ0FBQyxDQUFDLEdBQUcsQ0FBQyxDQUFDLENBQUMsQ0FBQyxDQUFDLEVBQUUsQ0FBQztBQUMxRyxJQUFJLE9BQU8sYUFBYSxDQUFDLENBQUMsRUFBRSxDQUFDLENBQUMsQ0FBQztBQUMvQixDQUFDLENBQUM7QUFDRjtBQUNPLFNBQVMsU0FBUyxDQUFDLENBQUMsRUFBRSxDQUFDLEVBQUU7QUFDaEMsSUFBSSxhQUFhLENBQUMsQ0FBQyxFQUFFLENBQUMsQ0FBQyxDQUFDO0FBQ3hCLElBQUksU0FBUyxFQUFFLEdBQUcsRUFBRSxJQUFJLENBQUMsV0FBVyxHQUFHLENBQUMsQ0FBQyxFQUFFO0FBQzNDLElBQUksQ0FBQyxDQUFDLFNBQVMsR0FBRyxDQUFDLEtBQUssSUFBSSxHQUFHLE1BQU0sQ0FBQyxNQUFNLENBQUMsQ0FBQyxDQUFDLElBQUksRUFBRSxDQUFDLFNBQVMsR0FBRyxDQUFDLENBQUMsU0FBUyxFQUFFLElBQUksRUFBRSxFQUFFLENBQUMsQ0FBQztBQUN6RixDQUFDO0FBdUNEO0FBQ08sU0FBUyxTQUFTLENBQUMsT0FBTyxFQUFFLFVBQVUsRUFBRSxDQUFDLEVBQUUsU0FBUyxFQUFFO0FBQzdELElBQUksU0FBUyxLQUFLLENBQUMsS0FBSyxFQUFFLEVBQUUsT0FBTyxLQUFLLFlBQVksQ0FBQyxHQUFHLEtBQUssR0FBRyxJQUFJLENBQUMsQ0FBQyxVQUFVLE9BQU8sRUFBRSxFQUFFLE9BQU8sQ0FBQyxLQUFLLENBQUMsQ0FBQyxFQUFFLENBQUMsQ0FBQyxFQUFFO0FBQ2hILElBQUksT0FBTyxLQUFLLENBQUMsS0FBSyxDQUFDLEdBQUcsT0FBTyxDQUFDLEVBQUUsVUFBVSxPQUFPLEVBQUUsTUFBTSxFQUFFO0FBQy9ELFFBQVEsU0FBUyxTQUFTLENBQUMsS0FBSyxFQUFFLEVBQUUsSUFBSSxFQUFFLElBQUksQ0FBQyxTQUFTLENBQUMsSUFBSSxDQUFDLEtBQUssQ0FBQyxDQUFDLENBQUMsRUFBRSxDQUFDLE9BQU8sQ0FBQyxFQUFFLEVBQUUsTUFBTSxDQUFDLENBQUMsQ0FBQyxDQUFDLEVBQUUsRUFBRTtBQUNuRyxRQUFRLFNBQVMsUUFBUSxDQUFDLEtBQUssRUFBRSxFQUFFLElBQUksRUFBRSxJQUFJLENBQUMsU0FBUyxDQUFDLE9BQU8sQ0FBQyxDQUFDLEtBQUssQ0FBQyxDQUFDLENBQUMsRUFBRSxDQUFDLE9BQU8sQ0FBQyxFQUFFLEVBQUUsTUFBTSxDQUFDLENBQUMsQ0FBQyxDQUFDLEVBQUUsRUFBRTtBQUN0RyxRQUFRLFNBQVMsSUFBSSxDQUFDLE1BQU0sRUFBRSxFQUFFLE1BQU0sQ0FBQyxJQUFJLEdBQUcsT0FBTyxDQUFDLE1BQU0sQ0FBQyxLQUFLLENBQUMsR0FBRyxLQUFLLENBQUMsTUFBTSxDQUFDLEtBQUssQ0FBQyxDQUFDLElBQUksQ0FBQyxTQUFTLEVBQUUsUUFBUSxDQUFDLENBQUMsRUFBRTtBQUN0SCxRQUFRLElBQUksQ0FBQyxDQUFDLFNBQVMsR0FBRyxTQUFTLENBQUMsS0FBSyxDQUFDLE9BQU8sRUFBRSxVQUFVLElBQUksRUFBRSxDQUFDLEVBQUUsSUFBSSxFQUFFLENBQUMsQ0FBQztBQUM5RSxLQUFLLENBQUMsQ0FBQztBQUNQLENBQUM7QUFDRDtBQUNPLFNBQVMsV0FBVyxDQUFDLE9BQU8sRUFBRSxJQUFJLEVBQUU7QUFDM0MsSUFBSSxJQUFJLENBQUMsR0FBRyxFQUFFLEtBQUssRUFBRSxDQUFDLEVBQUUsSUFBSSxFQUFFLFdBQVcsRUFBRSxJQUFJLENBQUMsQ0FBQyxDQUFDLENBQUMsR0FBRyxDQUFDLEVBQUUsTUFBTSxDQUFDLENBQUMsQ0FBQyxDQUFDLENBQUMsQ0FBQyxPQUFPLENBQUMsQ0FBQyxDQUFDLENBQUMsQ0FBQyxFQUFFLEVBQUUsSUFBSSxFQUFFLEVBQUUsRUFBRSxHQUFHLEVBQUUsRUFBRSxFQUFFLEVBQUUsQ0FBQyxFQUFFLENBQUMsRUFBRSxDQUFDLEVBQUUsQ0FBQyxDQUFDO0FBQ3JILElBQUksT0FBTyxDQUFDLEdBQUcsRUFBRSxJQUFJLEVBQUUsSUFBSSxDQUFDLENBQUMsQ0FBQyxFQUFFLE9BQU8sRUFBRSxJQUFJLENBQUMsQ0FBQyxDQUFDLEVBQUUsUUFBUSxFQUFFLElBQUksQ0FBQyxDQUFDLENBQUMsRUFBRSxFQUFFLE9BQU8sTUFBTSxLQUFLLFVBQVUsS0FBSyxDQUFDLENBQUMsTUFBTSxDQUFDLFFBQVEsQ0FBQyxHQUFHLFdBQVcsRUFBRSxPQUFPLElBQUksQ0FBQyxFQUFFLENBQUMsRUFBRSxDQUFDLENBQUM7QUFDN0osSUFBSSxTQUFTLElBQUksQ0FBQyxDQUFDLEVBQUUsRUFBRSxPQUFPLFVBQVUsQ0FBQyxFQUFFLEVBQUUsT0FBTyxJQUFJLENBQUMsQ0FBQyxDQUFDLEVBQUUsQ0FBQyxDQUFDLENBQUMsQ0FBQyxFQUFFLENBQUMsRUFBRTtBQUN0RSxJQUFJLFNBQVMsSUFBSSxDQUFDLEVBQUUsRUFBRTtBQUN0QixRQUFRLElBQUksQ0FBQyxFQUFFLE1BQU0sSUFBSSxTQUFTLENBQUMsaUNBQWlDLENBQUMsQ0FBQztBQUN0RSxRQUFRLE9BQU8sQ0FBQyxFQUFFLElBQUk7QUFDdEIsWUFBWSxJQUFJLENBQUMsR0FBRyxDQUFDLEVBQUUsQ0FBQyxLQUFLLENBQUMsR0FBRyxFQUFFLENBQUMsQ0FBQyxDQUFDLEdBQUcsQ0FBQyxHQUFHLENBQUMsQ0FBQyxRQUFRLENBQUMsR0FBRyxFQUFFLENBQUMsQ0FBQyxDQUFDLEdBQUcsQ0FBQyxDQUFDLE9BQU8sQ0FBQyxLQUFLLENBQUMsQ0FBQyxHQUFHLENBQUMsQ0FBQyxRQUFRLENBQUMsS0FBSyxDQUFDLENBQUMsSUFBSSxDQUFDLENBQUMsQ0FBQyxFQUFFLENBQUMsQ0FBQyxHQUFHLENBQUMsQ0FBQyxJQUFJLENBQUMsSUFBSSxDQUFDLENBQUMsQ0FB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

'use strict';

var obsidian = require('obsidian');

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

var __assign = function() {
    __assign = Object.assign || function __assign(t) {
        for (var s, i = 1, n = arguments.length; i < n; i++) {
            s = arguments[i];
            for (var p in s) if (Object.prototype.hasOwnProperty.call(s, p)) t[p] = s[p];
        }
        return t;
    };
    return __assign.apply(this, arguments);
};

function __rest(s, e) {
    var t = {};
    for (var p in s) if (Object.prototype.hasOwnProperty.call(s, p) && e.indexOf(p) < 0)
        t[p] = s[p];
    if (s != null && typeof Object.getOwnPropertySymbols === "function")
        for (var i = 0, p = Object.getOwnPropertySymbols(s); i < p.length; i++) {
            if (e.indexOf(p[i]) < 0 && Object.prototype.propertyIsEnumerable.call(s, p[i]))
                t[p[i]] = s[p[i]];
        }
    return t;
}

function __decorate(decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
}

function __param(paramIndex, decorator) {
    return function (target, key) { decorator(target, key, paramIndex); }
}

function __metadata(metadataKey, metadataValue) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(metadataKey, metadataValue);
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

var __createBinding = Object.create ? (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    Object.defineProperty(o, k2, { enumerable: true, get: function() { return m[k]; } });
}) : (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    o[k2] = m[k];
});

function __exportStar(m, o) {
    for (var p in m) if (p !== "default" && !Object.prototype.hasOwnProperty.call(o, p)) __createBinding(o, m, p);
}

function __values(o) {
    var s = typeof Symbol === "function" && Symbol.iterator, m = s && o[s], i = 0;
    if (m) return m.call(o);
    if (o && typeof o.length === "number") return {
        next: function () {
            if (o && i >= o.length) o = void 0;
            return { value: o && o[i++], done: !o };
        }
    };
    throw new TypeError(s ? "Object is not iterable." : "Symbol.iterator is not defined.");
}

function __read(o, n) {
    var m = typeof Symbol === "function" && o[Symbol.iterator];
    if (!m) return o;
    var i = m.call(o), r, ar = [], e;
    try {
        while ((n === void 0 || n-- > 0) && !(r = i.next()).done) ar.push(r.value);
    }
    catch (error) { e = { error: error }; }
    finally {
        try {
            if (r && !r.done && (m = i["return"])) m.call(i);
        }
        finally { if (e) throw e.error; }
    }
    return ar;
}

function __spread() {
    for (var ar = [], i = 0; i < arguments.length; i++)
        ar = ar.concat(__read(arguments[i]));
    return ar;
}

function __spreadArrays() {
    for (var s = 0, i = 0, il = arguments.length; i < il; i++) s += arguments[i].length;
    for (var r = Array(s), k = 0, i = 0; i < il; i++)
        for (var a = arguments[i], j = 0, jl = a.length; j < jl; j++, k++)
            r[k] = a[j];
    return r;
};

function __await(v) {
    return this instanceof __await ? (this.v = v, this) : new __await(v);
}

function __asyncGenerator(thisArg, _arguments, generator) {
    if (!Symbol.asyncIterator) throw new TypeError("Symbol.asyncIterator is not defined.");
    var g = generator.apply(thisArg, _arguments || []), i, q = [];
    return i = {}, verb("next"), verb("throw"), verb("return"), i[Symbol.asyncIterator] = function () { return this; }, i;
    function verb(n) { if (g[n]) i[n] = function (v) { return new Promise(function (a, b) { q.push([n, v, a, b]) > 1 || resume(n, v); }); }; }
    function resume(n, v) { try { step(g[n](v)); } catch (e) { settle(q[0][3], e); } }
    function step(r) { r.value instanceof __await ? Promise.resolve(r.value.v).then(fulfill, reject) : settle(q[0][2], r); }
    function fulfill(value) { resume("next", value); }
    function reject(value) { resume("throw", value); }
    function settle(f, v) { if (f(v), q.shift(), q.length) resume(q[0][0], q[0][1]); }
}

function __asyncDelegator(o) {
    var i, p;
    return i = {}, verb("next"), verb("throw", function (e) { throw e; }), verb("return"), i[Symbol.iterator] = function () { return this; }, i;
    function verb(n, f) { i[n] = o[n] ? function (v) { return (p = !p) ? { value: __await(o[n](v)), done: n === "return" } : f ? f(v) : v; } : f; }
}

function __asyncValues(o) {
    if (!Symbol.asyncIterator) throw new TypeError("Symbol.asyncIterator is not defined.");
    var m = o[Symbol.asyncIterator], i;
    return m ? m.call(o) : (o = typeof __values === "function" ? __values(o) : o[Symbol.iterator](), i = {}, verb("next"), verb("throw"), verb("return"), i[Symbol.asyncIterator] = function () { return this; }, i);
    function verb(n) { i[n] = o[n] && function (v) { return new Promise(function (resolve, reject) { v = o[n](v), settle(resolve, reject, v.done, v.value); }); }; }
    function settle(resolve, reject, d, v) { Promise.resolve(v).then(function(v) { resolve({ value: v, done: d }); }, reject); }
}

function __makeTemplateObject(cooked, raw) {
    if (Object.defineProperty) { Object.defineProperty(cooked, "raw", { value: raw }); } else { cooked.raw = raw; }
    return cooked;
};

var __setModuleDefault = Object.create ? (function(o, v) {
    Object.defineProperty(o, "default", { enumerable: true, value: v });
}) : function(o, v) {
    o["default"] = v;
};

function __importStar(mod) {
    if (mod && mod.__esModule) return mod;
    var result = {};
    if (mod != null) for (var k in mod) if (k !== "default" && Object.prototype.hasOwnProperty.call(mod, k)) __createBinding(result, mod, k);
    __setModuleDefault(result, mod);
    return result;
}

function __importDefault(mod) {
    return (mod && mod.__esModule) ? mod : { default: mod };
}

function __classPrivateFieldGet(receiver, privateMap) {
    if (!privateMap.has(receiver)) {
        throw new TypeError("attempted to get private field on non-instance");
    }
    return privateMap.get(receiver);
}

function __classPrivateFieldSet(receiver, privateMap, value) {
    if (!privateMap.has(receiver)) {
        throw new TypeError("attempted to set private field on non-instance");
    }
    privateMap.set(receiver, value);
    return value;
}

var grinning = {
	keywords: [
		"face",
		"smile",
		"happy",
		"joy",
		":D",
		"grin"
	],
	char: "😀",
	fitzpatrick_scale: false,
	category: "people"
};
var grimacing = {
	keywords: [
		"face",
		"grimace",
		"teeth"
	],
	char: "😬",
	fitzpatrick_scale: false,
	category: "people"
};
var grin = {
	keywords: [
		"face",
		"happy",
		"smile",
		"joy",
		"kawaii"
	],
	char: "😁",
	fitzpatrick_scale: false,
	category: "people"
};
var joy = {
	keywords: [
		"face",
		"cry",
		"tears",
		"weep",
		"happy",
		"happytears",
		"haha"
	],
	char: "😂",
	fitzpatrick_scale: false,
	category: "people"
};
var rofl = {
	keywords: [
		"face",
		"rolling",
		"floor",
		"laughing",
		"lol",
		"haha"
	],
	char: "🤣",
	fitzpatrick_scale: false,
	category: "people"
};
var partying = {
	keywords: [
		"face",
		"celebration",
		"woohoo"
	],
	char: "🥳",
	fitzpatrick_scale: false,
	category: "people"
};
var smiley = {
	keywords: [
		"face",
		"happy",
		"joy",
		"haha",
		":D",
		":)",
		"smile",
		"funny"
	],
	char: "😃",
	fitzpatrick_scale: false,
	category: "people"
};
var smile = {
	keywords: [
		"face",
		"happy",
		"joy",
		"funny",
		"haha",
		"laugh",
		"like",
		":D",
		":)"
	],
	char: "😄",
	fitzpatrick_scale: false,
	category: "people"
};
var sweat_smile = {
	keywords: [
		"face",
		"hot",
		"happy",
		"laugh",
		"sweat",
		"smile",
		"relief"
	],
	char: "😅",
	fitzpatrick_scale: false,
	category: "people"
};
var laughing = {
	keywords: [
		"happy",
		"joy",
		"lol",
		"satisfied",
		"haha",
		"face",
		"glad",
		"XD",
		"laugh"
	],
	char: "😆",
	fitzpatrick_scale: false,
	category: "people"
};
var innocent = {
	keywords: [
		"face",
		"angel",
		"heaven",
		"halo"
	],
	char: "😇",
	fitzpatrick_scale: false,
	category: "people"
};
var wink = {
	keywords: [
		"face",
		"happy",
		"mischievous",
		"secret",
		";)",
		"smile",
		"eye"
	],
	char: "😉",
	fitzpatrick_scale: false,
	category: "people"
};
var blush = {
	keywords: [
		"face",
		"smile",
		"happy",
		"flushed",
		"crush",
		"embarrassed",
		"shy",
		"joy"
	],
	char: "😊",
	fitzpatrick_scale: false,
	category: "people"
};
var slightly_smiling_face = {
	keywords: [
		"face",
		"smile"
	],
	char: "🙂",
	fitzpatrick_scale: false,
	category: "people"
};
var upside_down_face = {
	keywords: [
		"face",
		"flipped",
		"silly",
		"smile"
	],
	char: "🙃",
	fitzpatrick_scale: false,
	category: "people"
};
var relaxed = {
	keywords: [
		"face",
		"blush",
		"massage",
		"happiness"
	],
	char: "☺️",
	fitzpatrick_scale: false,
	category: "people"
};
var yum = {
	keywords: [
		"happy",
		"joy",
		"tongue",
		"smile",
		"face",
		"silly",
		"yummy",
		"nom",
		"delicious",
		"savouring"
	],
	char: "😋",
	fitzpatrick_scale: false,
	category: "people"
};
var relieved = {
	keywords: [
		"face",
		"relaxed",
		"phew",
		"massage",
		"happiness"
	],
	char: "😌",
	fitzpatrick_scale: false,
	category: "people"
};
var heart_eyes = {
	keywords: [
		"face",
		"love",
		"like",
		"affection",
		"valentines",
		"infatuation",
		"crush",
		"heart"
	],
	char: "😍",
	fitzpatrick_scale: false,
	category: "people"
};
var smiling_face_with_three_hearts = {
	keywords: [
		"face",
		"love",
		"like",
		"affection",
		"valentines",
		"infatuation",
		"crush",
		"hearts",
		"adore"
	],
	char: "🥰",
	fitzpatrick_scale: false,
	category: "people"
};
var kissing_heart = {
	keywords: [
		"face",
		"love",
		"like",
		"affection",
		"valentines",
		"infatuation",
		"kiss"
	],
	char: "😘",
	fitzpatrick_scale: false,
	category: "people"
};
var kissing = {
	keywords: [
		"love",
		"like",
		"face",
		"3",
		"valentines",
		"infatuation",
		"kiss"
	],
	char: "😗",
	fitzpatrick_scale: false,
	category: "people"
};
var kissing_smiling_eyes = {
	keywords: [
		"face",
		"affection",
		"valentines",
		"infatuation",
		"kiss"
	],
	char: "😙",
	fitzpatrick_scale: false,
	category: "people"
};
var kissing_closed_eyes = {
	keywords: [
		"face",
		"love",
		"like",
		"affection",
		"valentines",
		"infatuation",
		"kiss"
	],
	char: "😚",
	fitzpatrick_scale: false,
	category: "people"
};
var stuck_out_tongue_winking_eye = {
	keywords: [
		"face",
		"prank",
		"childish",
		"playful",
		"mischievous",
		"smile",
		"wink",
		"tongue"
	],
	char: "😜",
	fitzpatrick_scale: false,
	category: "people"
};
var zany = {
	keywords: [
		"face",
		"goofy",
		"crazy"
	],
	char: "🤪",
	fitzpatrick_scale: false,
	category: "people"
};
var raised_eyebrow = {
	keywords: [
		"face",
		"distrust",
		"scepticism",
		"disapproval",
		"disbelief",
		"surprise"
	],
	char: "🤨",
	fitzpatrick_scale: false,
	category: "people"
};
var monocle = {
	keywords: [
		"face",
		"stuffy",
		"wealthy"
	],
	char: "🧐",
	fitzpatrick_scale: false,
	category: "people"
};
var stuck_out_tongue_closed_eyes = {
	keywords: [
		"face",
		"prank",
		"playful",
		"mischievous",
		"smile",
		"tongue"
	],
	char: "😝",
	fitzpatrick_scale: false,
	category: "people"
};
var stuck_out_tongue = {
	keywords: [
		"face",
		"prank",
		"childish",
		"playful",
		"mischievous",
		"smile",
		"tongue"
	],
	char: "😛",
	fitzpatrick_scale: false,
	category: "people"
};
var money_mouth_face = {
	keywords: [
		"face",
		"rich",
		"dollar",
		"money"
	],
	char: "🤑",
	fitzpatrick_scale: false,
	category: "people"
};
var nerd_face = {
	keywords: [
		"face",
		"nerdy",
		"geek",
		"dork"
	],
	char: "🤓",
	fitzpatrick_scale: false,
	category: "people"
};
var sunglasses = {
	keywords: [
		"face",
		"cool",
		"smile",
		"summer",
		"beach",
		"sunglass"
	],
	char: "😎",
	fitzpatrick_scale: false,
	category: "people"
};
var star_struck = {
	keywords: [
		"face",
		"smile",
		"starry",
		"eyes",
		"grinning"
	],
	char: "🤩",
	fitzpatrick_scale: false,
	category: "people"
};
var clown_face = {
	keywords: [
		"face"
	],
	char: "🤡",
	fitzpatrick_scale: false,
	category: "people"
};
var cowboy_hat_face = {
	keywords: [
		"face",
		"cowgirl",
		"hat"
	],
	char: "🤠",
	fitzpatrick_scale: false,
	category: "people"
};
var hugs = {
	keywords: [
		"face",
		"smile",
		"hug"
	],
	char: "🤗",
	fitzpatrick_scale: false,
	category: "people"
};
var smirk = {
	keywords: [
		"face",
		"smile",
		"mean",
		"prank",
		"smug",
		"sarcasm"
	],
	char: "😏",
	fitzpatrick_scale: false,
	category: "people"
};
var no_mouth = {
	keywords: [
		"face",
		"hellokitty"
	],
	char: "😶",
	fitzpatrick_scale: false,
	category: "people"
};
var neutral_face = {
	keywords: [
		"indifference",
		"meh",
		":|",
		"neutral"
	],
	char: "😐",
	fitzpatrick_scale: false,
	category: "people"
};
var expressionless = {
	keywords: [
		"face",
		"indifferent",
		"-_-",
		"meh",
		"deadpan"
	],
	char: "😑",
	fitzpatrick_scale: false,
	category: "people"
};
var unamused = {
	keywords: [
		"indifference",
		"bored",
		"straight face",
		"serious",
		"sarcasm",
		"unimpressed",
		"skeptical",
		"dubious",
		"side_eye"
	],
	char: "😒",
	fitzpatrick_scale: false,
	category: "people"
};
var roll_eyes = {
	keywords: [
		"face",
		"eyeroll",
		"frustrated"
	],
	char: "🙄",
	fitzpatrick_scale: false,
	category: "people"
};
var thinking = {
	keywords: [
		"face",
		"hmmm",
		"think",
		"consider"
	],
	char: "🤔",
	fitzpatrick_scale: false,
	category: "people"
};
var lying_face = {
	keywords: [
		"face",
		"lie",
		"pinocchio"
	],
	char: "🤥",
	fitzpatrick_scale: false,
	category: "people"
};
var hand_over_mouth = {
	keywords: [
		"face",
		"whoops",
		"shock",
		"surprise"
	],
	char: "🤭",
	fitzpatrick_scale: false,
	category: "people"
};
var shushing = {
	keywords: [
		"face",
		"quiet",
		"shhh"
	],
	char: "🤫",
	fitzpatrick_scale: false,
	category: "people"
};
var symbols_over_mouth = {
	keywords: [
		"face",
		"swearing",
		"cursing",
		"cussing",
		"profanity",
		"expletive"
	],
	char: "🤬",
	fitzpatrick_scale: false,
	category: "people"
};
var exploding_head = {
	keywords: [
		"face",
		"shocked",
		"mind",
		"blown"
	],
	char: "🤯",
	fitzpatrick_scale: false,
	category: "people"
};
var flushed = {
	keywords: [
		"face",
		"blush",
		"shy",
		"flattered"
	],
	char: "😳",
	fitzpatrick_scale: false,
	category: "people"
};
var disappointed = {
	keywords: [
		"face",
		"sad",
		"upset",
		"depressed",
		":("
	],
	char: "😞",
	fitzpatrick_scale: false,
	category: "people"
};
var worried = {
	keywords: [
		"face",
		"concern",
		"nervous",
		":("
	],
	char: "😟",
	fitzpatrick_scale: false,
	category: "people"
};
var angry = {
	keywords: [
		"mad",
		"face",
		"annoyed",
		"frustrated"
	],
	char: "😠",
	fitzpatrick_scale: false,
	category: "people"
};
var rage = {
	keywords: [
		"angry",
		"mad",
		"hate",
		"despise"
	],
	char: "😡",
	fitzpatrick_scale: false,
	category: "people"
};
var pensive = {
	keywords: [
		"face",
		"sad",
		"depressed",
		"upset"
	],
	char: "😔",
	fitzpatrick_scale: false,
	category: "people"
};
var confused = {
	keywords: [
		"face",
		"indifference",
		"huh",
		"weird",
		"hmmm",
		":/"
	],
	char: "😕",
	fitzpatrick_scale: false,
	category: "people"
};
var slightly_frowning_face = {
	keywords: [
		"face",
		"frowning",
		"disappointed",
		"sad",
		"upset"
	],
	char: "🙁",
	fitzpatrick_scale: false,
	category: "people"
};
var frowning_face = {
	keywords: [
		"face",
		"sad",
		"upset",
		"frown"
	],
	char: "☹",
	fitzpatrick_scale: false,
	category: "people"
};
var persevere = {
	keywords: [
		"face",
		"sick",
		"no",
		"upset",
		"oops"
	],
	char: "😣",
	fitzpatrick_scale: false,
	category: "people"
};
var confounded = {
	keywords: [
		"face",
		"confused",
		"sick",
		"unwell",
		"oops",
		":S"
	],
	char: "😖",
	fitzpatrick_scale: false,
	category: "people"
};
var tired_face = {
	keywords: [
		"sick",
		"whine",
		"upset",
		"frustrated"
	],
	char: "😫",
	fitzpatrick_scale: false,
	category: "people"
};
var weary = {
	keywords: [
		"face",
		"tired",
		"sleepy",
		"sad",
		"frustrated",
		"upset"
	],
	char: "😩",
	fitzpatrick_scale: false,
	category: "people"
};
var pleading = {
	keywords: [
		"face",
		"begging",
		"mercy"
	],
	char: "🥺",
	fitzpatrick_scale: false,
	category: "people"
};
var triumph = {
	keywords: [
		"face",
		"gas",
		"phew",
		"proud",
		"pride"
	],
	char: "😤",
	fitzpatrick_scale: false,
	category: "people"
};
var open_mouth = {
	keywords: [
		"face",
		"surprise",
		"impressed",
		"wow",
		"whoa",
		":O"
	],
	char: "😮",
	fitzpatrick_scale: false,
	category: "people"
};
var scream = {
	keywords: [
		"face",
		"munch",
		"scared",
		"omg"
	],
	char: "😱",
	fitzpatrick_scale: false,
	category: "people"
};
var fearful = {
	keywords: [
		"face",
		"scared",
		"terrified",
		"nervous",
		"oops",
		"huh"
	],
	char: "😨",
	fitzpatrick_scale: false,
	category: "people"
};
var cold_sweat = {
	keywords: [
		"face",
		"nervous",
		"sweat"
	],
	char: "😰",
	fitzpatrick_scale: false,
	category: "people"
};
var hushed = {
	keywords: [
		"face",
		"woo",
		"shh"
	],
	char: "😯",
	fitzpatrick_scale: false,
	category: "people"
};
var frowning = {
	keywords: [
		"face",
		"aw",
		"what"
	],
	char: "😦",
	fitzpatrick_scale: false,
	category: "people"
};
var anguished = {
	keywords: [
		"face",
		"stunned",
		"nervous"
	],
	char: "😧",
	fitzpatrick_scale: false,
	category: "people"
};
var cry = {
	keywords: [
		"face",
		"tears",
		"sad",
		"depressed",
		"upset",
		":'("
	],
	char: "😢",
	fitzpatrick_scale: false,
	category: "people"
};
var disappointed_relieved = {
	keywords: [
		"face",
		"phew",
		"sweat",
		"nervous"
	],
	char: "😥",
	fitzpatrick_scale: false,
	category: "people"
};
var drooling_face = {
	keywords: [
		"face"
	],
	char: "🤤",
	fitzpatrick_scale: false,
	category: "people"
};
var sleepy = {
	keywords: [
		"face",
		"tired",
		"rest",
		"nap"
	],
	char: "😪",
	fitzpatrick_scale: false,
	category: "people"
};
var sweat = {
	keywords: [
		"face",
		"hot",
		"sad",
		"tired",
		"exercise"
	],
	char: "😓",
	fitzpatrick_scale: false,
	category: "people"
};
var hot = {
	keywords: [
		"face",
		"feverish",
		"heat",
		"red",
		"sweating"
	],
	char: "🥵",
	fitzpatrick_scale: false,
	category: "people"
};
var cold = {
	keywords: [
		"face",
		"blue",
		"freezing",
		"frozen",
		"frostbite",
		"icicles"
	],
	char: "🥶",
	fitzpatrick_scale: false,
	category: "people"
};
var sob = {
	keywords: [
		"face",
		"cry",
		"tears",
		"sad",
		"upset",
		"depressed"
	],
	char: "😭",
	fitzpatrick_scale: false,
	category: "people"
};
var dizzy_face = {
	keywords: [
		"spent",
		"unconscious",
		"xox",
		"dizzy"
	],
	char: "😵",
	fitzpatrick_scale: false,
	category: "people"
};
var astonished = {
	keywords: [
		"face",
		"xox",
		"surprised",
		"poisoned"
	],
	char: "😲",
	fitzpatrick_scale: false,
	category: "people"
};
var zipper_mouth_face = {
	keywords: [
		"face",
		"sealed",
		"zipper",
		"secret"
	],
	char: "🤐",
	fitzpatrick_scale: false,
	category: "people"
};
var nauseated_face = {
	keywords: [
		"face",
		"vomit",
		"gross",
		"green",
		"sick",
		"throw up",
		"ill"
	],
	char: "🤢",
	fitzpatrick_scale: false,
	category: "people"
};
var sneezing_face = {
	keywords: [
		"face",
		"gesundheit",
		"sneeze",
		"sick",
		"allergy"
	],
	char: "🤧",
	fitzpatrick_scale: false,
	category: "people"
};
var vomiting = {
	keywords: [
		"face",
		"sick"
	],
	char: "🤮",
	fitzpatrick_scale: false,
	category: "people"
};
var mask = {
	keywords: [
		"face",
		"sick",
		"ill",
		"disease"
	],
	char: "😷",
	fitzpatrick_scale: false,
	category: "people"
};
var face_with_thermometer = {
	keywords: [
		"sick",
		"temperature",
		"thermometer",
		"cold",
		"fever"
	],
	char: "🤒",
	fitzpatrick_scale: false,
	category: "people"
};
var face_with_head_bandage = {
	keywords: [
		"injured",
		"clumsy",
		"bandage",
		"hurt"
	],
	char: "🤕",
	fitzpatrick_scale: false,
	category: "people"
};
var woozy = {
	keywords: [
		"face",
		"dizzy",
		"intoxicated",
		"tipsy",
		"wavy"
	],
	char: "🥴",
	fitzpatrick_scale: false,
	category: "people"
};
var sleeping = {
	keywords: [
		"face",
		"tired",
		"sleepy",
		"night",
		"zzz"
	],
	char: "😴",
	fitzpatrick_scale: false,
	category: "people"
};
var zzz = {
	keywords: [
		"sleepy",
		"tired",
		"dream"
	],
	char: "💤",
	fitzpatrick_scale: false,
	category: "people"
};
var poop = {
	keywords: [
		"hankey",
		"shitface",
		"fail",
		"turd",
		"shit"
	],
	char: "💩",
	fitzpatrick_scale: false,
	category: "people"
};
var smiling_imp = {
	keywords: [
		"devil",
		"horns"
	],
	char: "😈",
	fitzpatrick_scale: false,
	category: "people"
};
var imp = {
	keywords: [
		"devil",
		"angry",
		"horns"
	],
	char: "👿",
	fitzpatrick_scale: false,
	category: "people"
};
var japanese_ogre = {
	keywords: [
		"monster",
		"red",
		"mask",
		"halloween",
		"scary",
		"creepy",
		"devil",
		"demon",
		"japanese",
		"ogre"
	],
	char: "👹",
	fitzpatrick_scale: false,
	category: "people"
};
var japanese_goblin = {
	keywords: [
		"red",
		"evil",
		"mask",
		"monster",
		"scary",
		"creepy",
		"japanese",
		"goblin"
	],
	char: "👺",
	fitzpatrick_scale: false,
	category: "people"
};
var skull = {
	keywords: [
		"dead",
		"skeleton",
		"creepy",
		"death"
	],
	char: "💀",
	fitzpatrick_scale: false,
	category: "people"
};
var ghost = {
	keywords: [
		"halloween",
		"spooky",
		"scary"
	],
	char: "👻",
	fitzpatrick_scale: false,
	category: "people"
};
var alien = {
	keywords: [
		"UFO",
		"paul",
		"weird",
		"outer_space"
	],
	char: "👽",
	fitzpatrick_scale: false,
	category: "people"
};
var robot = {
	keywords: [
		"computer",
		"machine",
		"bot"
	],
	char: "🤖",
	fitzpatrick_scale: false,
	category: "people"
};
var smiley_cat = {
	keywords: [
		"animal",
		"cats",
		"happy",
		"smile"
	],
	char: "😺",
	fitzpatrick_scale: false,
	category: "people"
};
var smile_cat = {
	keywords: [
		"animal",
		"cats",
		"smile"
	],
	char: "😸",
	fitzpatrick_scale: false,
	category: "people"
};
var joy_cat = {
	keywords: [
		"animal",
		"cats",
		"haha",
		"happy",
		"tears"
	],
	char: "😹",
	fitzpatrick_scale: false,
	category: "people"
};
var heart_eyes_cat = {
	keywords: [
		"animal",
		"love",
		"like",
		"affection",
		"cats",
		"valentines",
		"heart"
	],
	char: "😻",
	fitzpatrick_scale: false,
	category: "people"
};
var smirk_cat = {
	keywords: [
		"animal",
		"cats",
		"smirk"
	],
	char: "😼",
	fitzpatrick_scale: false,
	category: "people"
};
var kissing_cat = {
	keywords: [
		"animal",
		"cats",
		"kiss"
	],
	char: "😽",
	fitzpatrick_scale: false,
	category: "people"
};
var scream_cat = {
	keywords: [
		"animal",
		"cats",
		"munch",
		"scared",
		"scream"
	],
	char: "🙀",
	fitzpatrick_scale: false,
	category: "people"
};
var crying_cat_face = {
	keywords: [
		"animal",
		"tears",
		"weep",
		"sad",
		"cats",
		"upset",
		"cry"
	],
	char: "😿",
	fitzpatrick_scale: false,
	category: "people"
};
var pouting_cat = {
	keywords: [
		"animal",
		"cats"
	],
	char: "😾",
	fitzpatrick_scale: false,
	category: "people"
};
var palms_up = {
	keywords: [
		"hands",
		"gesture",
		"cupped",
		"prayer"
	],
	char: "🤲",
	fitzpatrick_scale: true,
	category: "people"
};
var raised_hands = {
	keywords: [
		"gesture",
		"hooray",
		"yea",
		"celebration",
		"hands"
	],
	char: "🙌",
	fitzpatrick_scale: true,
	category: "people"
};
var clap = {
	keywords: [
		"hands",
		"praise",
		"applause",
		"congrats",
		"yay"
	],
	char: "👏",
	fitzpatrick_scale: true,
	category: "people"
};
var wave = {
	keywords: [
		"hands",
		"gesture",
		"goodbye",
		"solong",
		"farewell",
		"hello",
		"hi",
		"palm"
	],
	char: "👋",
	fitzpatrick_scale: true,
	category: "people"
};
var call_me_hand = {
	keywords: [
		"hands",
		"gesture"
	],
	char: "🤙",
	fitzpatrick_scale: true,
	category: "people"
};
var facepunch = {
	keywords: [
		"angry",
		"violence",
		"fist",
		"hit",
		"attack",
		"hand"
	],
	char: "👊",
	fitzpatrick_scale: true,
	category: "people"
};
var fist = {
	keywords: [
		"fingers",
		"hand",
		"grasp"
	],
	char: "✊",
	fitzpatrick_scale: true,
	category: "people"
};
var fist_left = {
	keywords: [
		"hand",
		"fistbump"
	],
	char: "🤛",
	fitzpatrick_scale: true,
	category: "people"
};
var fist_right = {
	keywords: [
		"hand",
		"fistbump"
	],
	char: "🤜",
	fitzpatrick_scale: true,
	category: "people"
};
var v = {
	keywords: [
		"fingers",
		"ohyeah",
		"hand",
		"peace",
		"victory",
		"two"
	],
	char: "✌",
	fitzpatrick_scale: true,
	category: "people"
};
var ok_hand = {
	keywords: [
		"fingers",
		"limbs",
		"perfect",
		"ok",
		"okay"
	],
	char: "👌",
	fitzpatrick_scale: true,
	category: "people"
};
var raised_hand = {
	keywords: [
		"fingers",
		"stop",
		"highfive",
		"palm",
		"ban"
	],
	char: "✋",
	fitzpatrick_scale: true,
	category: "people"
};
var raised_back_of_hand = {
	keywords: [
		"fingers",
		"raised",
		"backhand"
	],
	char: "🤚",
	fitzpatrick_scale: true,
	category: "people"
};
var open_hands = {
	keywords: [
		"fingers",
		"butterfly",
		"hands",
		"open"
	],
	char: "👐",
	fitzpatrick_scale: true,
	category: "people"
};
var muscle = {
	keywords: [
		"arm",
		"flex",
		"hand",
		"summer",
		"strong",
		"biceps"
	],
	char: "💪",
	fitzpatrick_scale: true,
	category: "people"
};
var pray = {
	keywords: [
		"please",
		"hope",
		"wish",
		"namaste",
		"highfive"
	],
	char: "🙏",
	fitzpatrick_scale: true,
	category: "people"
};
var foot = {
	keywords: [
		"kick",
		"stomp"
	],
	char: "🦶",
	fitzpatrick_scale: true,
	category: "people"
};
var leg = {
	keywords: [
		"kick",
		"limb"
	],
	char: "🦵",
	fitzpatrick_scale: true,
	category: "people"
};
var handshake = {
	keywords: [
		"agreement",
		"shake"
	],
	char: "🤝",
	fitzpatrick_scale: false,
	category: "people"
};
var point_up = {
	keywords: [
		"hand",
		"fingers",
		"direction",
		"up"
	],
	char: "☝",
	fitzpatrick_scale: true,
	category: "people"
};
var point_up_2 = {
	keywords: [
		"fingers",
		"hand",
		"direction",
		"up"
	],
	char: "👆",
	fitzpatrick_scale: true,
	category: "people"
};
var point_down = {
	keywords: [
		"fingers",
		"hand",
		"direction",
		"down"
	],
	char: "👇",
	fitzpatrick_scale: true,
	category: "people"
};
var point_left = {
	keywords: [
		"direction",
		"fingers",
		"hand",
		"left"
	],
	char: "👈",
	fitzpatrick_scale: true,
	category: "people"
};
var point_right = {
	keywords: [
		"fingers",
		"hand",
		"direction",
		"right"
	],
	char: "👉",
	fitzpatrick_scale: true,
	category: "people"
};
var fu = {
	keywords: [
		"hand",
		"fingers",
		"rude",
		"middle",
		"flipping"
	],
	char: "🖕",
	fitzpatrick_scale: true,
	category: "people"
};
var raised_hand_with_fingers_splayed = {
	keywords: [
		"hand",
		"fingers",
		"palm"
	],
	char: "🖐",
	fitzpatrick_scale: true,
	category: "people"
};
var love_you = {
	keywords: [
		"hand",
		"fingers",
		"gesture"
	],
	char: "🤟",
	fitzpatrick_scale: true,
	category: "people"
};
var metal = {
	keywords: [
		"hand",
		"fingers",
		"evil_eye",
		"sign_of_horns",
		"rock_on"
	],
	char: "🤘",
	fitzpatrick_scale: true,
	category: "people"
};
var crossed_fingers = {
	keywords: [
		"good",
		"lucky"
	],
	char: "🤞",
	fitzpatrick_scale: true,
	category: "people"
};
var vulcan_salute = {
	keywords: [
		"hand",
		"fingers",
		"spock",
		"star trek"
	],
	char: "🖖",
	fitzpatrick_scale: true,
	category: "people"
};
var writing_hand = {
	keywords: [
		"lower_left_ballpoint_pen",
		"stationery",
		"write",
		"compose"
	],
	char: "✍",
	fitzpatrick_scale: true,
	category: "people"
};
var selfie = {
	keywords: [
		"camera",
		"phone"
	],
	char: "🤳",
	fitzpatrick_scale: true,
	category: "people"
};
var nail_care = {
	keywords: [
		"beauty",
		"manicure",
		"finger",
		"fashion",
		"nail"
	],
	char: "💅",
	fitzpatrick_scale: true,
	category: "people"
};
var lips = {
	keywords: [
		"mouth",
		"kiss"
	],
	char: "👄",
	fitzpatrick_scale: false,
	category: "people"
};
var tooth = {
	keywords: [
		"teeth",
		"dentist"
	],
	char: "🦷",
	fitzpatrick_scale: false,
	category: "people"
};
var tongue = {
	keywords: [
		"mouth",
		"playful"
	],
	char: "👅",
	fitzpatrick_scale: false,
	category: "people"
};
var ear = {
	keywords: [
		"face",
		"hear",
		"sound",
		"listen"
	],
	char: "👂",
	fitzpatrick_scale: true,
	category: "people"
};
var nose = {
	keywords: [
		"smell",
		"sniff"
	],
	char: "👃",
	fitzpatrick_scale: true,
	category: "people"
};
var eye = {
	keywords: [
		"face",
		"look",
		"see",
		"watch",
		"stare"
	],
	char: "👁",
	fitzpatrick_scale: false,
	category: "people"
};
var eyes = {
	keywords: [
		"look",
		"watch",
		"stalk",
		"peek",
		"see"
	],
	char: "👀",
	fitzpatrick_scale: false,
	category: "people"
};
var brain = {
	keywords: [
		"smart",
		"intelligent"
	],
	char: "🧠",
	fitzpatrick_scale: false,
	category: "people"
};
var bust_in_silhouette = {
	keywords: [
		"user",
		"person",
		"human"
	],
	char: "👤",
	fitzpatrick_scale: false,
	category: "people"
};
var busts_in_silhouette = {
	keywords: [
		"user",
		"person",
		"human",
		"group",
		"team"
	],
	char: "👥",
	fitzpatrick_scale: false,
	category: "people"
};
var speaking_head = {
	keywords: [
		"user",
		"person",
		"human",
		"sing",
		"say",
		"talk"
	],
	char: "🗣",
	fitzpatrick_scale: false,
	category: "people"
};
var baby = {
	keywords: [
		"child",
		"boy",
		"girl",
		"toddler"
	],
	char: "👶",
	fitzpatrick_scale: true,
	category: "people"
};
var child = {
	keywords: [
		"gender-neutral",
		"young"
	],
	char: "🧒",
	fitzpatrick_scale: true,
	category: "people"
};
var boy = {
	keywords: [
		"man",
		"male",
		"guy",
		"teenager"
	],
	char: "👦",
	fitzpatrick_scale: true,
	category: "people"
};
var girl = {
	keywords: [
		"female",
		"woman",
		"teenager"
	],
	char: "👧",
	fitzpatrick_scale: true,
	category: "people"
};
var adult = {
	keywords: [
		"gender-neutral",
		"person"
	],
	char: "🧑",
	fitzpatrick_scale: true,
	category: "people"
};
var man = {
	keywords: [
		"mustache",
		"father",
		"dad",
		"guy",
		"classy",
		"sir",
		"moustache"
	],
	char: "👨",
	fitzpatrick_scale: true,
	category: "people"
};
var woman = {
	keywords: [
		"female",
		"girls",
		"lady"
	],
	char: "👩",
	fitzpatrick_scale: true,
	category: "people"
};
var blonde_woman = {
	keywords: [
		"woman",
		"female",
		"girl",
		"blonde",
		"person"
	],
	char: "👱‍♀️",
	fitzpatrick_scale: true,
	category: "people"
};
var blonde_man = {
	keywords: [
		"man",
		"male",
		"boy",
		"blonde",
		"guy",
		"person"
	],
	char: "👱",
	fitzpatrick_scale: true,
	category: "people"
};
var bearded_person = {
	keywords: [
		"person",
		"bewhiskered"
	],
	char: "🧔",
	fitzpatrick_scale: true,
	category: "people"
};
var older_adult = {
	keywords: [
		"human",
		"elder",
		"senior",
		"gender-neutral"
	],
	char: "🧓",
	fitzpatrick_scale: true,
	category: "people"
};
var older_man = {
	keywords: [
		"human",
		"male",
		"men",
		"old",
		"elder",
		"senior"
	],
	char: "👴",
	fitzpatrick_scale: true,
	category: "people"
};
var older_woman = {
	keywords: [
		"human",
		"female",
		"women",
		"lady",
		"old",
		"elder",
		"senior"
	],
	char: "👵",
	fitzpatrick_scale: true,
	category: "people"
};
var man_with_gua_pi_mao = {
	keywords: [
		"male",
		"boy",
		"chinese"
	],
	char: "👲",
	fitzpatrick_scale: true,
	category: "people"
};
var woman_with_headscarf = {
	keywords: [
		"female",
		"hijab",
		"mantilla",
		"tichel"
	],
	char: "🧕",
	fitzpatrick_scale: true,
	category: "people"
};
var woman_with_turban = {
	keywords: [
		"female",
		"indian",
		"hinduism",
		"arabs",
		"woman"
	],
	char: "👳‍♀️",
	fitzpatrick_scale: true,
	category: "people"
};
var man_with_turban = {
	keywords: [
		"male",
		"indian",
		"hinduism",
		"arabs"
	],
	char: "👳",
	fitzpatrick_scale: true,
	category: "people"
};
var policewoman = {
	keywords: [
		"woman",
		"police",
		"law",
		"legal",
		"enforcement",
		"arrest",
		"911",
		"female"
	],
	char: "👮‍♀️",
	fitzpatrick_scale: true,
	category: "people"
};
var policeman = {
	keywords: [
		"man",
		"police",
		"law",
		"legal",
		"enforcement",
		"arrest",
		"911"
	],
	char: "👮",
	fitzpatrick_scale: true,
	category: "people"
};
var construction_worker_woman = {
	keywords: [
		"female",
		"human",
		"wip",
		"build",
		"construction",
		"worker",
		"labor",
		"woman"
	],
	char: "👷‍♀️",
	fitzpatrick_scale: true,
	category: "people"
};
var construction_worker_man = {
	keywords: [
		"male",
		"human",
		"wip",
		"guy",
		"build",
		"construction",
		"worker",
		"labor"
	],
	char: "👷",
	fitzpatrick_scale: true,
	category: "people"
};
var guardswoman = {
	keywords: [
		"uk",
		"gb",
		"british",
		"female",
		"royal",
		"woman"
	],
	char: "💂‍♀️",
	fitzpatrick_scale: true,
	category: "people"
};
var guardsman = {
	keywords: [
		"uk",
		"gb",
		"british",
		"male",
		"guy",
		"royal"
	],
	char: "💂",
	fitzpatrick_scale: true,
	category: "people"
};
var female_detective = {
	keywords: [
		"human",
		"spy",
		"detective",
		"female",
		"woman"
	],
	char: "🕵️‍♀️",
	fitzpatrick_scale: true,
	category: "people"
};
var male_detective = {
	keywords: [
		"human",
		"spy",
		"detective"
	],
	char: "🕵",
	fitzpatrick_scale: true,
	category: "people"
};
var woman_health_worker = {
	keywords: [
		"doctor",
		"nurse",
		"therapist",
		"healthcare",
		"woman",
		"human"
	],
	char: "👩‍⚕️",
	fitzpatrick_scale: true,
	category: "people"
};
var man_health_worker = {
	keywords: [
		"doctor",
		"nurse",
		"therapist",
		"healthcare",
		"man",
		"human"
	],
	char: "👨‍⚕️",
	fitzpatrick_scale: true,
	category: "people"
};
var woman_farmer = {
	keywords: [
		"rancher",
		"gardener",
		"woman",
		"human"
	],
	char: "👩‍🌾",
	fitzpatrick_scale: true,
	category: "people"
};
var man_farmer = {
	keywords: [
		"rancher",
		"gardener",
		"man",
		"human"
	],
	char: "👨‍🌾",
	fitzpatrick_scale: true,
	category: "people"
};
var woman_cook = {
	keywords: [
		"chef",
		"woman",
		"human"
	],
	char: "👩‍🍳",
	fitzpatrick_scale: true,
	category: "people"
};
var man_cook = {
	keywords: [
		"chef",
		"man",
		"human"
	],
	char: "👨‍🍳",
	fitzpatrick_scale: true,
	category: "people"
};
var woman_student = {
	keywords: [
		"graduate",
		"woman",
		"human"
	],
	char: "👩‍🎓",
	fitzpatrick_scale: true,
	category: "people"
};
var man_student = {
	keywords: [
		"graduate",
		"man",
		"human"
	],
	char: "👨‍🎓",
	fitzpatrick_scale: true,
	category: "people"
};
var woman_singer = {
	keywords: [
		"rockstar",
		"entertainer",
		"woman",
		"human"
	],
	char: "👩‍🎤",
	fitzpatrick_scale: true,
	category: "people"
};
var man_singer = {
	keywords: [
		"rockstar",
		"entertainer",
		"man",
		"human"
	],
	char: "👨‍🎤",
	fitzpatrick_scale: true,
	category: "people"
};
var woman_teacher = {
	keywords: [
		"instructor",
		"professor",
		"woman",
		"human"
	],
	char: "👩‍🏫",
	fitzpatrick_scale: true,
	category: "people"
};
var man_teacher = {
	keywords: [
		"instructor",
		"professor",
		"man",
		"human"
	],
	char: "👨‍🏫",
	fitzpatrick_scale: true,
	category: "people"
};
var woman_factory_worker = {
	keywords: [
		"assembly",
		"industrial",
		"woman",
		"human"
	],
	char: "👩‍🏭",
	fitzpatrick_scale: true,
	category: "people"
};
var man_factory_worker = {
	keywords: [
		"assembly",
		"industrial",
		"man",
		"human"
	],
	char: "👨‍🏭",
	fitzpatrick_scale: true,
	category: "people"
};
var woman_technologist = {
	keywords: [
		"coder",
		"developer",
		"engineer",
		"programmer",
		"software",
		"woman",
		"human",
		"laptop",
		"computer"
	],
	char: "👩‍💻",
	fitzpatrick_scale: true,
	category: "people"
};
var man_technologist = {
	keywords: [
		"coder",
		"developer",
		"engineer",
		"programmer",
		"software",
		"man",
		"human",
		"laptop",
		"computer"
	],
	char: "👨‍💻",
	fitzpatrick_scale: true,
	category: "people"
};
var woman_office_worker = {
	keywords: [
		"business",
		"manager",
		"woman",
		"human"
	],
	char: "👩‍💼",
	fitzpatrick_scale: true,
	category: "people"
};
var man_office_worker = {
	keywords: [
		"business",
		"manager",
		"man",
		"human"
	],
	char: "👨‍💼",
	fitzpatrick_scale: true,
	category: "people"
};
var woman_mechanic = {
	keywords: [
		"plumber",
		"woman",
		"human",
		"wrench"
	],
	char: "👩‍🔧",
	fitzpatrick_scale: true,
	category: "people"
};
var man_mechanic = {
	keywords: [
		"plumber",
		"man",
		"human",
		"wrench"
	],
	char: "👨‍🔧",
	fitzpatrick_scale: true,
	category: "people"
};
var woman_scientist = {
	keywords: [
		"biologist",
		"chemist",
		"engineer",
		"physicist",
		"woman",
		"human"
	],
	char: "👩‍🔬",
	fitzpatrick_scale: true,
	category: "people"
};
var man_scientist = {
	keywords: [
		"biologist",
		"chemist",
		"engineer",
		"physicist",
		"man",
		"human"
	],
	char: "👨‍🔬",
	fitzpatrick_scale: true,
	category: "people"
};
var woman_artist = {
	keywords: [
		"painter",
		"woman",
		"human"
	],
	char: "👩‍🎨",
	fitzpatrick_scale: true,
	category: "people"
};
var man_artist = {
	keywords: [
		"painter",
		"man",
		"human"
	],
	char: "👨‍🎨",
	fitzpatrick_scale: true,
	category: "people"
};
var woman_firefighter = {
	keywords: [
		"fireman",
		"woman",
		"human"
	],
	char: "👩‍🚒",
	fitzpatrick_scale: true,
	category: "people"
};
var man_firefighter = {
	keywords: [
		"fireman",
		"man",
		"human"
	],
	char: "👨‍🚒",
	fitzpatrick_scale: true,
	category: "people"
};
var woman_pilot = {
	keywords: [
		"aviator",
		"plane",
		"woman",
		"human"
	],
	char: "👩‍✈️",
	fitzpatrick_scale: true,
	category: "people"
};
var man_pilot = {
	keywords: [
		"aviator",
		"plane",
		"man",
		"human"
	],
	char: "👨‍✈️",
	fitzpatrick_scale: true,
	category: "people"
};
var woman_astronaut = {
	keywords: [
		"space",
		"rocket",
		"woman",
		"human"
	],
	char: "👩‍🚀",
	fitzpatrick_scale: true,
	category: "people"
};
var man_astronaut = {
	keywords: [
		"space",
		"rocket",
		"man",
		"human"
	],
	char: "👨‍🚀",
	fitzpatrick_scale: true,
	category: "people"
};
var woman_judge = {
	keywords: [
		"justice",
		"court",
		"woman",
		"human"
	],
	char: "👩‍⚖️",
	fitzpatrick_scale: true,
	category: "people"
};
var man_judge = {
	keywords: [
		"justice",
		"court",
		"man",
		"human"
	],
	char: "👨‍⚖️",
	fitzpatrick_scale: true,
	category: "people"
};
var woman_superhero = {
	keywords: [
		"woman",
		"female",
		"good",
		"heroine",
		"superpowers"
	],
	char: "🦸‍♀️",
	fitzpatrick_scale: true,
	category: "people"
};
var man_superhero = {
	keywords: [
		"man",
		"male",
		"good",
		"hero",
		"superpowers"
	],
	char: "🦸‍♂️",
	fitzpatrick_scale: true,
	category: "people"
};
var woman_supervillain = {
	keywords: [
		"woman",
		"female",
		"evil",
		"bad",
		"criminal",
		"heroine",
		"superpowers"
	],
	char: "🦹‍♀️",
	fitzpatrick_scale: true,
	category: "people"
};
var man_supervillain = {
	keywords: [
		"man",
		"male",
		"evil",
		"bad",
		"criminal",
		"hero",
		"superpowers"
	],
	char: "🦹‍♂️",
	fitzpatrick_scale: true,
	category: "people"
};
var mrs_claus = {
	keywords: [
		"woman",
		"female",
		"xmas",
		"mother christmas"
	],
	char: "🤶",
	fitzpatrick_scale: true,
	category: "people"
};
var santa = {
	keywords: [
		"festival",
		"man",
		"male",
		"xmas",
		"father christmas"
	],
	char: "🎅",
	fitzpatrick_scale: true,
	category: "people"
};
var sorceress = {
	keywords: [
		"woman",
		"female",
		"mage",
		"witch"
	],
	char: "🧙‍♀️",
	fitzpatrick_scale: true,
	category: "people"
};
var wizard = {
	keywords: [
		"man",
		"male",
		"mage",
		"sorcerer"
	],
	char: "🧙‍♂️",
	fitzpatrick_scale: true,
	category: "people"
};
var woman_elf = {
	keywords: [
		"woman",
		"female"
	],
	char: "🧝‍♀️",
	fitzpatrick_scale: true,
	category: "people"
};
var man_elf = {
	keywords: [
		"man",
		"male"
	],
	char: "🧝‍♂️",
	fitzpatrick_scale: true,
	category: "people"
};
var woman_vampire = {
	keywords: [
		"woman",
		"female"
	],
	char: "🧛‍♀️",
	fitzpatrick_scale: true,
	category: "people"
};
var man_vampire = {
	keywords: [
		"man",
		"male",
		"dracula"
	],
	char: "🧛‍♂️",
	fitzpatrick_scale: true,
	category: "people"
};
var woman_zombie = {
	keywords: [
		"woman",
		"female",
		"undead",
		"walking dead"
	],
	char: "🧟‍♀️",
	fitzpatrick_scale: false,
	category: "people"
};
var man_zombie = {
	keywords: [
		"man",
		"male",
		"dracula",
		"undead",
		"walking dead"
	],
	char: "🧟‍♂️",
	fitzpatrick_scale: false,
	category: "people"
};
var woman_genie = {
	keywords: [
		"woman",
		"female"
	],
	char: "🧞‍♀️",
	fitzpatrick_scale: false,
	category: "people"
};
var man_genie = {
	keywords: [
		"man",
		"male"
	],
	char: "🧞‍♂️",
	fitzpatrick_scale: false,
	category: "people"
};
var mermaid = {
	keywords: [
		"woman",
		"female",
		"merwoman",
		"ariel"
	],
	char: "🧜‍♀️",
	fitzpatrick_scale: true,
	category: "people"
};
var merman = {
	keywords: [
		"man",
		"male",
		"triton"
	],
	char: "🧜‍♂️",
	fitzpatrick_scale: true,
	category: "people"
};
var woman_fairy = {
	keywords: [
		"woman",
		"female"
	],
	char: "🧚‍♀️",
	fitzpatrick_scale: true,
	category: "people"
};
var man_fairy = {
	keywords: [
		"man",
		"male"
	],
	char: "🧚‍♂️",
	fitzpatrick_scale: true,
	category: "people"
};
var angel = {
	keywords: [
		"heaven",
		"wings",
		"halo"
	],
	char: "👼",
	fitzpatrick_scale: true,
	category: "people"
};
var pregnant_woman = {
	keywords: [
		"baby"
	],
	char: "🤰",
	fitzpatrick_scale: true,
	category: "people"
};
var breastfeeding = {
	keywords: [
		"nursing",
		"baby"
	],
	char: "🤱",
	fitzpatrick_scale: true,
	category: "people"
};
var princess = {
	keywords: [
		"girl",
		"woman",
		"female",
		"blond",
		"crown",
		"royal",
		"queen"
	],
	char: "👸",
	fitzpatrick_scale: true,
	category: "people"
};
var prince = {
	keywords: [
		"boy",
		"man",
		"male",
		"crown",
		"royal",
		"king"
	],
	char: "🤴",
	fitzpatrick_scale: true,
	category: "people"
};
var bride_with_veil = {
	keywords: [
		"couple",
		"marriage",
		"wedding",
		"woman",
		"bride"
	],
	char: "👰",
	fitzpatrick_scale: true,
	category: "people"
};
var man_in_tuxedo = {
	keywords: [
		"couple",
		"marriage",
		"wedding",
		"groom"
	],
	char: "🤵",
	fitzpatrick_scale: true,
	category: "people"
};
var running_woman = {
	keywords: [
		"woman",
		"walking",
		"exercise",
		"race",
		"running",
		"female"
	],
	char: "🏃‍♀️",
	fitzpatrick_scale: true,
	category: "people"
};
var running_man = {
	keywords: [
		"man",
		"walking",
		"exercise",
		"race",
		"running"
	],
	char: "🏃",
	fitzpatrick_scale: true,
	category: "people"
};
var walking_woman = {
	keywords: [
		"human",
		"feet",
		"steps",
		"woman",
		"female"
	],
	char: "🚶‍♀️",
	fitzpatrick_scale: true,
	category: "people"
};
var walking_man = {
	keywords: [
		"human",
		"feet",
		"steps"
	],
	char: "🚶",
	fitzpatrick_scale: true,
	category: "people"
};
var dancer = {
	keywords: [
		"female",
		"girl",
		"woman",
		"fun"
	],
	char: "💃",
	fitzpatrick_scale: true,
	category: "people"
};
var man_dancing = {
	keywords: [
		"male",
		"boy",
		"fun",
		"dancer"
	],
	char: "🕺",
	fitzpatrick_scale: true,
	category: "people"
};
var dancing_women = {
	keywords: [
		"female",
		"bunny",
		"women",
		"girls"
	],
	char: "👯",
	fitzpatrick_scale: false,
	category: "people"
};
var dancing_men = {
	keywords: [
		"male",
		"bunny",
		"men",
		"boys"
	],
	char: "👯‍♂️",
	fitzpatrick_scale: false,
	category: "people"
};
var couple = {
	keywords: [
		"pair",
		"people",
		"human",
		"love",
		"date",
		"dating",
		"like",
		"affection",
		"valentines",
		"marriage"
	],
	char: "👫",
	fitzpatrick_scale: false,
	category: "people"
};
var two_men_holding_hands = {
	keywords: [
		"pair",
		"couple",
		"love",
		"like",
		"bromance",
		"friendship",
		"people",
		"human"
	],
	char: "👬",
	fitzpatrick_scale: false,
	category: "people"
};
var two_women_holding_hands = {
	keywords: [
		"pair",
		"friendship",
		"couple",
		"love",
		"like",
		"female",
		"people",
		"human"
	],
	char: "👭",
	fitzpatrick_scale: false,
	category: "people"
};
var bowing_woman = {
	keywords: [
		"woman",
		"female",
		"girl"
	],
	char: "🙇‍♀️",
	fitzpatrick_scale: true,
	category: "people"
};
var bowing_man = {
	keywords: [
		"man",
		"male",
		"boy"
	],
	char: "🙇",
	fitzpatrick_scale: true,
	category: "people"
};
var man_facepalming = {
	keywords: [
		"man",
		"male",
		"boy",
		"disbelief"
	],
	char: "🤦‍♂️",
	fitzpatrick_scale: true,
	category: "people"
};
var woman_facepalming = {
	keywords: [
		"woman",
		"female",
		"girl",
		"disbelief"
	],
	char: "🤦‍♀️",
	fitzpatrick_scale: true,
	category: "people"
};
var woman_shrugging = {
	keywords: [
		"woman",
		"female",
		"girl",
		"confused",
		"indifferent",
		"doubt"
	],
	char: "🤷",
	fitzpatrick_scale: true,
	category: "people"
};
var man_shrugging = {
	keywords: [
		"man",
		"male",
		"boy",
		"confused",
		"indifferent",
		"doubt"
	],
	char: "🤷‍♂️",
	fitzpatrick_scale: true,
	category: "people"
};
var tipping_hand_woman = {
	keywords: [
		"female",
		"girl",
		"woman",
		"human",
		"information"
	],
	char: "💁",
	fitzpatrick_scale: true,
	category: "people"
};
var tipping_hand_man = {
	keywords: [
		"male",
		"boy",
		"man",
		"human",
		"information"
	],
	char: "💁‍♂️",
	fitzpatrick_scale: true,
	category: "people"
};
var no_good_woman = {
	keywords: [
		"female",
		"girl",
		"woman",
		"nope"
	],
	char: "🙅",
	fitzpatrick_scale: true,
	category: "people"
};
var no_good_man = {
	keywords: [
		"male",
		"boy",
		"man",
		"nope"
	],
	char: "🙅‍♂️",
	fitzpatrick_scale: true,
	category: "people"
};
var ok_woman = {
	keywords: [
		"women",
		"girl",
		"female",
		"pink",
		"human",
		"woman"
	],
	char: "🙆",
	fitzpatrick_scale: true,
	category: "people"
};
var ok_man = {
	keywords: [
		"men",
		"boy",
		"male",
		"blue",
		"human",
		"man"
	],
	char: "🙆‍♂️",
	fitzpatrick_scale: true,
	category: "people"
};
var raising_hand_woman = {
	keywords: [
		"female",
		"girl",
		"woman"
	],
	char: "🙋",
	fitzpatrick_scale: true,
	category: "people"
};
var raising_hand_man = {
	keywords: [
		"male",
		"boy",
		"man"
	],
	char: "🙋‍♂️",
	fitzpatrick_scale: true,
	category: "people"
};
var pouting_woman = {
	keywords: [
		"female",
		"girl",
		"woman"
	],
	char: "🙎",
	fitzpatrick_scale: true,
	category: "people"
};
var pouting_man = {
	keywords: [
		"male",
		"boy",
		"man"
	],
	char: "🙎‍♂️",
	fitzpatrick_scale: true,
	category: "people"
};
var frowning_woman = {
	keywords: [
		"female",
		"girl",
		"woman",
		"sad",
		"depressed",
		"discouraged",
		"unhappy"
	],
	char: "🙍",
	fitzpatrick_scale: true,
	category: "people"
};
var frowning_man = {
	keywords: [
		"male",
		"boy",
		"man",
		"sad",
		"depressed",
		"discouraged",
		"unhappy"
	],
	char: "🙍‍♂️",
	fitzpatrick_scale: true,
	category: "people"
};
var haircut_woman = {
	keywords: [
		"female",
		"girl",
		"woman"
	],
	char: "💇",
	fitzpatrick_scale: true,
	category: "people"
};
var haircut_man = {
	keywords: [
		"male",
		"boy",
		"man"
	],
	char: "💇‍♂️",
	fitzpatrick_scale: true,
	category: "people"
};
var massage_woman = {
	keywords: [
		"female",
		"girl",
		"woman",
		"head"
	],
	char: "💆",
	fitzpatrick_scale: true,
	category: "people"
};
var massage_man = {
	keywords: [
		"male",
		"boy",
		"man",
		"head"
	],
	char: "💆‍♂️",
	fitzpatrick_scale: true,
	category: "people"
};
var woman_in_steamy_room = {
	keywords: [
		"female",
		"woman",
		"spa",
		"steamroom",
		"sauna"
	],
	char: "🧖‍♀️",
	fitzpatrick_scale: true,
	category: "people"
};
var man_in_steamy_room = {
	keywords: [
		"male",
		"man",
		"spa",
		"steamroom",
		"sauna"
	],
	char: "🧖‍♂️",
	fitzpatrick_scale: true,
	category: "people"
};
var couple_with_heart_woman_man = {
	keywords: [
		"pair",
		"love",
		"like",
		"affection",
		"human",
		"dating",
		"valentines",
		"marriage"
	],
	char: "💑",
	fitzpatrick_scale: false,
	category: "people"
};
var couple_with_heart_woman_woman = {
	keywords: [
		"pair",
		"love",
		"like",
		"affection",
		"human",
		"dating",
		"valentines",
		"marriage"
	],
	char: "👩‍❤️‍👩",
	fitzpatrick_scale: false,
	category: "people"
};
var couple_with_heart_man_man = {
	keywords: [
		"pair",
		"love",
		"like",
		"affection",
		"human",
		"dating",
		"valentines",
		"marriage"
	],
	char: "👨‍❤️‍👨",
	fitzpatrick_scale: false,
	category: "people"
};
var couplekiss_man_woman = {
	keywords: [
		"pair",
		"valentines",
		"love",
		"like",
		"dating",
		"marriage"
	],
	char: "💏",
	fitzpatrick_scale: false,
	category: "people"
};
var couplekiss_woman_woman = {
	keywords: [
		"pair",
		"valentines",
		"love",
		"like",
		"dating",
		"marriage"
	],
	char: "👩‍❤️‍💋‍👩",
	fitzpatrick_scale: false,
	category: "people"
};
var couplekiss_man_man = {
	keywords: [
		"pair",
		"valentines",
		"love",
		"like",
		"dating",
		"marriage"
	],
	char: "👨‍❤️‍💋‍👨",
	fitzpatrick_scale: false,
	category: "people"
};
var family_man_woman_boy = {
	keywords: [
		"home",
		"parents",
		"child",
		"mom",
		"dad",
		"father",
		"mother",
		"people",
		"human"
	],
	char: "👪",
	fitzpatrick_scale: false,
	category: "people"
};
var family_man_woman_girl = {
	keywords: [
		"home",
		"parents",
		"people",
		"human",
		"child"
	],
	char: "👨‍👩‍👧",
	fitzpatrick_scale: false,
	category: "people"
};
var family_man_woman_girl_boy = {
	keywords: [
		"home",
		"parents",
		"people",
		"human",
		"children"
	],
	char: "👨‍👩‍👧‍👦",
	fitzpatrick_scale: false,
	category: "people"
};
var family_man_woman_boy_boy = {
	keywords: [
		"home",
		"parents",
		"people",
		"human",
		"children"
	],
	char: "👨‍👩‍👦‍👦",
	fitzpatrick_scale: false,
	category: "people"
};
var family_man_woman_girl_girl = {
	keywords: [
		"home",
		"parents",
		"people",
		"human",
		"children"
	],
	char: "👨‍👩‍👧‍👧",
	fitzpatrick_scale: false,
	category: "people"
};
var family_woman_woman_boy = {
	keywords: [
		"home",
		"parents",
		"people",
		"human",
		"children"
	],
	char: "👩‍👩‍👦",
	fitzpatrick_scale: false,
	category: "people"
};
var family_woman_woman_girl = {
	keywords: [
		"home",
		"parents",
		"people",
		"human",
		"children"
	],
	char: "👩‍👩‍👧",
	fitzpatrick_scale: false,
	category: "people"
};
var family_woman_woman_girl_boy = {
	keywords: [
		"home",
		"parents",
		"people",
		"human",
		"children"
	],
	char: "👩‍👩‍👧‍👦",
	fitzpatrick_scale: false,
	category: "people"
};
var family_woman_woman_boy_boy = {
	keywords: [
		"home",
		"parents",
		"people",
		"human",
		"children"
	],
	char: "👩‍👩‍👦‍👦",
	fitzpatrick_scale: false,
	category: "people"
};
var family_woman_woman_girl_girl = {
	keywords: [
		"home",
		"parents",
		"people",
		"human",
		"children"
	],
	char: "👩‍👩‍👧‍👧",
	fitzpatrick_scale: false,
	category: "people"
};
var family_man_man_boy = {
	keywords: [
		"home",
		"parents",
		"people",
		"human",
		"children"
	],
	char: "👨‍👨‍👦",
	fitzpatrick_scale: false,
	category: "people"
};
var family_man_man_girl = {
	keywords: [
		"home",
		"parents",
		"people",
		"human",
		"children"
	],
	char: "👨‍👨‍👧",
	fitzpatrick_scale: false,
	category: "people"
};
var family_man_man_girl_boy = {
	keywords: [
		"home",
		"parents",
		"people",
		"human",
		"children"
	],
	char: "👨‍👨‍👧‍👦",
	fitzpatrick_scale: false,
	category: "people"
};
var family_man_man_boy_boy = {
	keywords: [
		"home",
		"parents",
		"people",
		"human",
		"children"
	],
	char: "👨‍👨‍👦‍👦",
	fitzpatrick_scale: false,
	category: "people"
};
var family_man_man_girl_girl = {
	keywords: [
		"home",
		"parents",
		"people",
		"human",
		"children"
	],
	char: "👨‍👨‍👧‍👧",
	fitzpatrick_scale: false,
	category: "people"
};
var family_woman_boy = {
	keywords: [
		"home",
		"parent",
		"people",
		"human",
		"child"
	],
	char: "👩‍👦",
	fitzpatrick_scale: false,
	category: "people"
};
var family_woman_girl = {
	keywords: [
		"home",
		"parent",
		"people",
		"human",
		"child"
	],
	char: "👩‍👧",
	fitzpatrick_scale: false,
	category: "people"
};
var family_woman_girl_boy = {
	keywords: [
		"home",
		"parent",
		"people",
		"human",
		"children"
	],
	char: "👩‍👧‍👦",
	fitzpatrick_scale: false,
	category: "people"
};
var family_woman_boy_boy = {
	keywords: [
		"home",
		"parent",
		"people",
		"human",
		"children"
	],
	char: "👩‍👦‍👦",
	fitzpatrick_scale: false,
	category: "people"
};
var family_woman_girl_girl = {
	keywords: [
		"home",
		"parent",
		"people",
		"human",
		"children"
	],
	char: "👩‍👧‍👧",
	fitzpatrick_scale: false,
	category: "people"
};
var family_man_boy = {
	keywords: [
		"home",
		"parent",
		"people",
		"human",
		"child"
	],
	char: "👨‍👦",
	fitzpatrick_scale: false,
	category: "people"
};
var family_man_girl = {
	keywords: [
		"home",
		"parent",
		"people",
		"human",
		"child"
	],
	char: "👨‍👧",
	fitzpatrick_scale: false,
	category: "people"
};
var family_man_girl_boy = {
	keywords: [
		"home",
		"parent",
		"people",
		"human",
		"children"
	],
	char: "👨‍👧‍👦",
	fitzpatrick_scale: false,
	category: "people"
};
var family_man_boy_boy = {
	keywords: [
		"home",
		"parent",
		"people",
		"human",
		"children"
	],
	char: "👨‍👦‍👦",
	fitzpatrick_scale: false,
	category: "people"
};
var family_man_girl_girl = {
	keywords: [
		"home",
		"parent",
		"people",
		"human",
		"children"
	],
	char: "👨‍👧‍👧",
	fitzpatrick_scale: false,
	category: "people"
};
var yarn = {
	keywords: [
		"ball",
		"crochet",
		"knit"
	],
	char: "🧶",
	fitzpatrick_scale: false,
	category: "people"
};
var thread = {
	keywords: [
		"needle",
		"sewing",
		"spool",
		"string"
	],
	char: "🧵",
	fitzpatrick_scale: false,
	category: "people"
};
var coat = {
	keywords: [
		"jacket"
	],
	char: "🧥",
	fitzpatrick_scale: false,
	category: "people"
};
var labcoat = {
	keywords: [
		"doctor",
		"experiment",
		"scientist",
		"chemist"
	],
	char: "🥼",
	fitzpatrick_scale: false,
	category: "people"
};
var womans_clothes = {
	keywords: [
		"fashion",
		"shopping_bags",
		"female"
	],
	char: "👚",
	fitzpatrick_scale: false,
	category: "people"
};
var tshirt = {
	keywords: [
		"fashion",
		"cloth",
		"casual",
		"shirt",
		"tee"
	],
	char: "👕",
	fitzpatrick_scale: false,
	category: "people"
};
var jeans = {
	keywords: [
		"fashion",
		"shopping"
	],
	char: "👖",
	fitzpatrick_scale: false,
	category: "people"
};
var necktie = {
	keywords: [
		"shirt",
		"suitup",
		"formal",
		"fashion",
		"cloth",
		"business"
	],
	char: "👔",
	fitzpatrick_scale: false,
	category: "people"
};
var dress = {
	keywords: [
		"clothes",
		"fashion",
		"shopping"
	],
	char: "👗",
	fitzpatrick_scale: false,
	category: "people"
};
var bikini = {
	keywords: [
		"swimming",
		"female",
		"woman",
		"girl",
		"fashion",
		"beach",
		"summer"
	],
	char: "👙",
	fitzpatrick_scale: false,
	category: "people"
};
var kimono = {
	keywords: [
		"dress",
		"fashion",
		"women",
		"female",
		"japanese"
	],
	char: "👘",
	fitzpatrick_scale: false,
	category: "people"
};
var lipstick = {
	keywords: [
		"female",
		"girl",
		"fashion",
		"woman"
	],
	char: "💄",
	fitzpatrick_scale: false,
	category: "people"
};
var kiss = {
	keywords: [
		"face",
		"lips",
		"love",
		"like",
		"affection",
		"valentines"
	],
	char: "💋",
	fitzpatrick_scale: false,
	category: "people"
};
var footprints = {
	keywords: [
		"feet",
		"tracking",
		"walking",
		"beach"
	],
	char: "👣",
	fitzpatrick_scale: false,
	category: "people"
};
var flat_shoe = {
	keywords: [
		"ballet",
		"slip-on",
		"slipper"
	],
	char: "🥿",
	fitzpatrick_scale: false,
	category: "people"
};
var high_heel = {
	keywords: [
		"fashion",
		"shoes",
		"female",
		"pumps",
		"stiletto"
	],
	char: "👠",
	fitzpatrick_scale: false,
	category: "people"
};
var sandal = {
	keywords: [
		"shoes",
		"fashion",
		"flip flops"
	],
	char: "👡",
	fitzpatrick_scale: false,
	category: "people"
};
var boot = {
	keywords: [
		"shoes",
		"fashion"
	],
	char: "👢",
	fitzpatrick_scale: false,
	category: "people"
};
var mans_shoe = {
	keywords: [
		"fashion",
		"male"
	],
	char: "👞",
	fitzpatrick_scale: false,
	category: "people"
};
var athletic_shoe = {
	keywords: [
		"shoes",
		"sports",
		"sneakers"
	],
	char: "👟",
	fitzpatrick_scale: false,
	category: "people"
};
var hiking_boot = {
	keywords: [
		"backpacking",
		"camping",
		"hiking"
	],
	char: "🥾",
	fitzpatrick_scale: false,
	category: "people"
};
var socks = {
	keywords: [
		"stockings",
		"clothes"
	],
	char: "🧦",
	fitzpatrick_scale: false,
	category: "people"
};
var gloves = {
	keywords: [
		"hands",
		"winter",
		"clothes"
	],
	char: "🧤",
	fitzpatrick_scale: false,
	category: "people"
};
var scarf = {
	keywords: [
		"neck",
		"winter",
		"clothes"
	],
	char: "🧣",
	fitzpatrick_scale: false,
	category: "people"
};
var womans_hat = {
	keywords: [
		"fashion",
		"accessories",
		"female",
		"lady",
		"spring"
	],
	char: "👒",
	fitzpatrick_scale: false,
	category: "people"
};
var tophat = {
	keywords: [
		"magic",
		"gentleman",
		"classy",
		"circus"
	],
	char: "🎩",
	fitzpatrick_scale: false,
	category: "people"
};
var billed_hat = {
	keywords: [
		"cap",
		"baseball"
	],
	char: "🧢",
	fitzpatrick_scale: false,
	category: "people"
};
var rescue_worker_helmet = {
	keywords: [
		"construction",
		"build"
	],
	char: "⛑",
	fitzpatrick_scale: false,
	category: "people"
};
var mortar_board = {
	keywords: [
		"school",
		"college",
		"degree",
		"university",
		"graduation",
		"cap",
		"hat",
		"legal",
		"learn",
		"education"
	],
	char: "🎓",
	fitzpatrick_scale: false,
	category: "people"
};
var crown = {
	keywords: [
		"king",
		"kod",
		"leader",
		"royalty",
		"lord"
	],
	char: "👑",
	fitzpatrick_scale: false,
	category: "people"
};
var school_satchel = {
	keywords: [
		"student",
		"education",
		"bag",
		"backpack"
	],
	char: "🎒",
	fitzpatrick_scale: false,
	category: "people"
};
var luggage = {
	keywords: [
		"packing",
		"travel"
	],
	char: "🧳",
	fitzpatrick_scale: false,
	category: "people"
};
var pouch = {
	keywords: [
		"bag",
		"accessories",
		"shopping"
	],
	char: "👝",
	fitzpatrick_scale: false,
	category: "people"
};
var purse = {
	keywords: [
		"fashion",
		"accessories",
		"money",
		"sales",
		"shopping"
	],
	char: "👛",
	fitzpatrick_scale: false,
	category: "people"
};
var handbag = {
	keywords: [
		"fashion",
		"accessory",
		"accessories",
		"shopping"
	],
	char: "👜",
	fitzpatrick_scale: false,
	category: "people"
};
var briefcase = {
	keywords: [
		"business",
		"documents",
		"work",
		"law",
		"legal",
		"job",
		"career"
	],
	char: "💼",
	fitzpatrick_scale: false,
	category: "people"
};
var eyeglasses = {
	keywords: [
		"fashion",
		"accessories",
		"eyesight",
		"nerdy",
		"dork",
		"geek"
	],
	char: "👓",
	fitzpatrick_scale: false,
	category: "people"
};
var dark_sunglasses = {
	keywords: [
		"face",
		"cool",
		"accessories"
	],
	char: "🕶",
	fitzpatrick_scale: false,
	category: "people"
};
var goggles = {
	keywords: [
		"eyes",
		"protection",
		"safety"
	],
	char: "🥽",
	fitzpatrick_scale: false,
	category: "people"
};
var ring = {
	keywords: [
		"wedding",
		"propose",
		"marriage",
		"valentines",
		"diamond",
		"fashion",
		"jewelry",
		"gem",
		"engagement"
	],
	char: "💍",
	fitzpatrick_scale: false,
	category: "people"
};
var closed_umbrella = {
	keywords: [
		"weather",
		"rain",
		"drizzle"
	],
	char: "🌂",
	fitzpatrick_scale: false,
	category: "people"
};
var dog = {
	keywords: [
		"animal",
		"friend",
		"nature",
		"woof",
		"puppy",
		"pet",
		"faithful"
	],
	char: "🐶",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var cat = {
	keywords: [
		"animal",
		"meow",
		"nature",
		"pet",
		"kitten"
	],
	char: "🐱",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var mouse = {
	keywords: [
		"animal",
		"nature",
		"cheese_wedge",
		"rodent"
	],
	char: "🐭",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var hamster = {
	keywords: [
		"animal",
		"nature"
	],
	char: "🐹",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var rabbit = {
	keywords: [
		"animal",
		"nature",
		"pet",
		"spring",
		"magic",
		"bunny"
	],
	char: "🐰",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var fox_face = {
	keywords: [
		"animal",
		"nature",
		"face"
	],
	char: "🦊",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var bear = {
	keywords: [
		"animal",
		"nature",
		"wild"
	],
	char: "🐻",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var panda_face = {
	keywords: [
		"animal",
		"nature",
		"panda"
	],
	char: "🐼",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var koala = {
	keywords: [
		"animal",
		"nature"
	],
	char: "🐨",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var tiger = {
	keywords: [
		"animal",
		"cat",
		"danger",
		"wild",
		"nature",
		"roar"
	],
	char: "🐯",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var lion = {
	keywords: [
		"animal",
		"nature"
	],
	char: "🦁",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var cow = {
	keywords: [
		"beef",
		"ox",
		"animal",
		"nature",
		"moo",
		"milk"
	],
	char: "🐮",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var pig = {
	keywords: [
		"animal",
		"oink",
		"nature"
	],
	char: "🐷",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var pig_nose = {
	keywords: [
		"animal",
		"oink"
	],
	char: "🐽",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var frog = {
	keywords: [
		"animal",
		"nature",
		"croak",
		"toad"
	],
	char: "🐸",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var squid = {
	keywords: [
		"animal",
		"nature",
		"ocean",
		"sea"
	],
	char: "🦑",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var octopus = {
	keywords: [
		"animal",
		"creature",
		"ocean",
		"sea",
		"nature",
		"beach"
	],
	char: "🐙",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var shrimp = {
	keywords: [
		"animal",
		"ocean",
		"nature",
		"seafood"
	],
	char: "🦐",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var monkey_face = {
	keywords: [
		"animal",
		"nature",
		"circus"
	],
	char: "🐵",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var gorilla = {
	keywords: [
		"animal",
		"nature",
		"circus"
	],
	char: "🦍",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var see_no_evil = {
	keywords: [
		"monkey",
		"animal",
		"nature",
		"haha"
	],
	char: "🙈",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var hear_no_evil = {
	keywords: [
		"animal",
		"monkey",
		"nature"
	],
	char: "🙉",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var speak_no_evil = {
	keywords: [
		"monkey",
		"animal",
		"nature",
		"omg"
	],
	char: "🙊",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var monkey = {
	keywords: [
		"animal",
		"nature",
		"banana",
		"circus"
	],
	char: "🐒",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var chicken = {
	keywords: [
		"animal",
		"cluck",
		"nature",
		"bird"
	],
	char: "🐔",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var penguin = {
	keywords: [
		"animal",
		"nature"
	],
	char: "🐧",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var bird = {
	keywords: [
		"animal",
		"nature",
		"fly",
		"tweet",
		"spring"
	],
	char: "🐦",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var baby_chick = {
	keywords: [
		"animal",
		"chicken",
		"bird"
	],
	char: "🐤",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var hatching_chick = {
	keywords: [
		"animal",
		"chicken",
		"egg",
		"born",
		"baby",
		"bird"
	],
	char: "🐣",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var hatched_chick = {
	keywords: [
		"animal",
		"chicken",
		"baby",
		"bird"
	],
	char: "🐥",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var duck = {
	keywords: [
		"animal",
		"nature",
		"bird",
		"mallard"
	],
	char: "🦆",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var eagle = {
	keywords: [
		"animal",
		"nature",
		"bird"
	],
	char: "🦅",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var owl = {
	keywords: [
		"animal",
		"nature",
		"bird",
		"hoot"
	],
	char: "🦉",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var bat = {
	keywords: [
		"animal",
		"nature",
		"blind",
		"vampire"
	],
	char: "🦇",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var wolf = {
	keywords: [
		"animal",
		"nature",
		"wild"
	],
	char: "🐺",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var boar = {
	keywords: [
		"animal",
		"nature"
	],
	char: "🐗",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var horse = {
	keywords: [
		"animal",
		"brown",
		"nature"
	],
	char: "🐴",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var unicorn = {
	keywords: [
		"animal",
		"nature",
		"mystical"
	],
	char: "🦄",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var honeybee = {
	keywords: [
		"animal",
		"insect",
		"nature",
		"bug",
		"spring",
		"honey"
	],
	char: "🐝",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var bug = {
	keywords: [
		"animal",
		"insect",
		"nature",
		"worm"
	],
	char: "🐛",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var butterfly = {
	keywords: [
		"animal",
		"insect",
		"nature",
		"caterpillar"
	],
	char: "🦋",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var snail = {
	keywords: [
		"slow",
		"animal",
		"shell"
	],
	char: "🐌",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var beetle = {
	keywords: [
		"animal",
		"insect",
		"nature",
		"ladybug"
	],
	char: "🐞",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var ant = {
	keywords: [
		"animal",
		"insect",
		"nature",
		"bug"
	],
	char: "🐜",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var grasshopper = {
	keywords: [
		"animal",
		"cricket",
		"chirp"
	],
	char: "🦗",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var spider = {
	keywords: [
		"animal",
		"arachnid"
	],
	char: "🕷",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var scorpion = {
	keywords: [
		"animal",
		"arachnid"
	],
	char: "🦂",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var crab = {
	keywords: [
		"animal",
		"crustacean"
	],
	char: "🦀",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var snake = {
	keywords: [
		"animal",
		"evil",
		"nature",
		"hiss",
		"python"
	],
	char: "🐍",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var lizard = {
	keywords: [
		"animal",
		"nature",
		"reptile"
	],
	char: "🦎",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var sauropod = {
	keywords: [
		"animal",
		"nature",
		"dinosaur",
		"brachiosaurus",
		"brontosaurus",
		"diplodocus",
		"extinct"
	],
	char: "🦕",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var turtle = {
	keywords: [
		"animal",
		"slow",
		"nature",
		"tortoise"
	],
	char: "🐢",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var tropical_fish = {
	keywords: [
		"animal",
		"swim",
		"ocean",
		"beach",
		"nemo"
	],
	char: "🐠",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var fish = {
	keywords: [
		"animal",
		"food",
		"nature"
	],
	char: "🐟",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var blowfish = {
	keywords: [
		"animal",
		"nature",
		"food",
		"sea",
		"ocean"
	],
	char: "🐡",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var dolphin = {
	keywords: [
		"animal",
		"nature",
		"fish",
		"sea",
		"ocean",
		"flipper",
		"fins",
		"beach"
	],
	char: "🐬",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var shark = {
	keywords: [
		"animal",
		"nature",
		"fish",
		"sea",
		"ocean",
		"jaws",
		"fins",
		"beach"
	],
	char: "🦈",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var whale = {
	keywords: [
		"animal",
		"nature",
		"sea",
		"ocean"
	],
	char: "🐳",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var whale2 = {
	keywords: [
		"animal",
		"nature",
		"sea",
		"ocean"
	],
	char: "🐋",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var crocodile = {
	keywords: [
		"animal",
		"nature",
		"reptile",
		"lizard",
		"alligator"
	],
	char: "🐊",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var leopard = {
	keywords: [
		"animal",
		"nature"
	],
	char: "🐆",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var zebra = {
	keywords: [
		"animal",
		"nature",
		"stripes",
		"safari"
	],
	char: "🦓",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var tiger2 = {
	keywords: [
		"animal",
		"nature",
		"roar"
	],
	char: "🐅",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var water_buffalo = {
	keywords: [
		"animal",
		"nature",
		"ox",
		"cow"
	],
	char: "🐃",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var ox = {
	keywords: [
		"animal",
		"cow",
		"beef"
	],
	char: "🐂",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var cow2 = {
	keywords: [
		"beef",
		"ox",
		"animal",
		"nature",
		"moo",
		"milk"
	],
	char: "🐄",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var deer = {
	keywords: [
		"animal",
		"nature",
		"horns",
		"venison"
	],
	char: "🦌",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var dromedary_camel = {
	keywords: [
		"animal",
		"hot",
		"desert",
		"hump"
	],
	char: "🐪",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var camel = {
	keywords: [
		"animal",
		"nature",
		"hot",
		"desert",
		"hump"
	],
	char: "🐫",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var giraffe = {
	keywords: [
		"animal",
		"nature",
		"spots",
		"safari"
	],
	char: "🦒",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var elephant = {
	keywords: [
		"animal",
		"nature",
		"nose",
		"th",
		"circus"
	],
	char: "🐘",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var rhinoceros = {
	keywords: [
		"animal",
		"nature",
		"horn"
	],
	char: "🦏",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var goat = {
	keywords: [
		"animal",
		"nature"
	],
	char: "🐐",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var ram = {
	keywords: [
		"animal",
		"sheep",
		"nature"
	],
	char: "🐏",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var sheep = {
	keywords: [
		"animal",
		"nature",
		"wool",
		"shipit"
	],
	char: "🐑",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var racehorse = {
	keywords: [
		"animal",
		"gamble",
		"luck"
	],
	char: "🐎",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var pig2 = {
	keywords: [
		"animal",
		"nature"
	],
	char: "🐖",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var rat = {
	keywords: [
		"animal",
		"mouse",
		"rodent"
	],
	char: "🐀",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var mouse2 = {
	keywords: [
		"animal",
		"nature",
		"rodent"
	],
	char: "🐁",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var rooster = {
	keywords: [
		"animal",
		"nature",
		"chicken"
	],
	char: "🐓",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var turkey = {
	keywords: [
		"animal",
		"bird"
	],
	char: "🦃",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var dove = {
	keywords: [
		"animal",
		"bird"
	],
	char: "🕊",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var dog2 = {
	keywords: [
		"animal",
		"nature",
		"friend",
		"doge",
		"pet",
		"faithful"
	],
	char: "🐕",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var poodle = {
	keywords: [
		"dog",
		"animal",
		"101",
		"nature",
		"pet"
	],
	char: "🐩",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var cat2 = {
	keywords: [
		"animal",
		"meow",
		"pet",
		"cats"
	],
	char: "🐈",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var rabbit2 = {
	keywords: [
		"animal",
		"nature",
		"pet",
		"magic",
		"spring"
	],
	char: "🐇",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var chipmunk = {
	keywords: [
		"animal",
		"nature",
		"rodent",
		"squirrel"
	],
	char: "🐿",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var hedgehog = {
	keywords: [
		"animal",
		"nature",
		"spiny"
	],
	char: "🦔",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var raccoon = {
	keywords: [
		"animal",
		"nature"
	],
	char: "🦝",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var llama = {
	keywords: [
		"animal",
		"nature",
		"alpaca"
	],
	char: "🦙",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var hippopotamus = {
	keywords: [
		"animal",
		"nature"
	],
	char: "🦛",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var kangaroo = {
	keywords: [
		"animal",
		"nature",
		"australia",
		"joey",
		"hop",
		"marsupial"
	],
	char: "🦘",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var badger = {
	keywords: [
		"animal",
		"nature",
		"honey"
	],
	char: "🦡",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var swan = {
	keywords: [
		"animal",
		"nature",
		"bird"
	],
	char: "🦢",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var peacock = {
	keywords: [
		"animal",
		"nature",
		"peahen",
		"bird"
	],
	char: "🦚",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var parrot = {
	keywords: [
		"animal",
		"nature",
		"bird",
		"pirate",
		"talk"
	],
	char: "🦜",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var lobster = {
	keywords: [
		"animal",
		"nature",
		"bisque",
		"claws",
		"seafood"
	],
	char: "🦞",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var mosquito = {
	keywords: [
		"animal",
		"nature",
		"insect",
		"malaria"
	],
	char: "🦟",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var paw_prints = {
	keywords: [
		"animal",
		"tracking",
		"footprints",
		"dog",
		"cat",
		"pet",
		"feet"
	],
	char: "🐾",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var dragon = {
	keywords: [
		"animal",
		"myth",
		"nature",
		"chinese",
		"green"
	],
	char: "🐉",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var dragon_face = {
	keywords: [
		"animal",
		"myth",
		"nature",
		"chinese",
		"green"
	],
	char: "🐲",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var cactus = {
	keywords: [
		"vegetable",
		"plant",
		"nature"
	],
	char: "🌵",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var christmas_tree = {
	keywords: [
		"festival",
		"vacation",
		"december",
		"xmas",
		"celebration"
	],
	char: "🎄",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var evergreen_tree = {
	keywords: [
		"plant",
		"nature"
	],
	char: "🌲",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var deciduous_tree = {
	keywords: [
		"plant",
		"nature"
	],
	char: "🌳",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var palm_tree = {
	keywords: [
		"plant",
		"vegetable",
		"nature",
		"summer",
		"beach",
		"mojito",
		"tropical"
	],
	char: "🌴",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var seedling = {
	keywords: [
		"plant",
		"nature",
		"grass",
		"lawn",
		"spring"
	],
	char: "🌱",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var herb = {
	keywords: [
		"vegetable",
		"plant",
		"medicine",
		"weed",
		"grass",
		"lawn"
	],
	char: "🌿",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var shamrock = {
	keywords: [
		"vegetable",
		"plant",
		"nature",
		"irish",
		"clover"
	],
	char: "☘",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var four_leaf_clover = {
	keywords: [
		"vegetable",
		"plant",
		"nature",
		"lucky",
		"irish"
	],
	char: "🍀",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var bamboo = {
	keywords: [
		"plant",
		"nature",
		"vegetable",
		"panda",
		"pine_decoration"
	],
	char: "🎍",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var tanabata_tree = {
	keywords: [
		"plant",
		"nature",
		"branch",
		"summer"
	],
	char: "🎋",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var leaves = {
	keywords: [
		"nature",
		"plant",
		"tree",
		"vegetable",
		"grass",
		"lawn",
		"spring"
	],
	char: "🍃",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var fallen_leaf = {
	keywords: [
		"nature",
		"plant",
		"vegetable",
		"leaves"
	],
	char: "🍂",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var maple_leaf = {
	keywords: [
		"nature",
		"plant",
		"vegetable",
		"ca",
		"fall"
	],
	char: "🍁",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var ear_of_rice = {
	keywords: [
		"nature",
		"plant"
	],
	char: "🌾",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var hibiscus = {
	keywords: [
		"plant",
		"vegetable",
		"flowers",
		"beach"
	],
	char: "🌺",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var sunflower = {
	keywords: [
		"nature",
		"plant",
		"fall"
	],
	char: "🌻",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var rose = {
	keywords: [
		"flowers",
		"valentines",
		"love",
		"spring"
	],
	char: "🌹",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var wilted_flower = {
	keywords: [
		"plant",
		"nature",
		"flower"
	],
	char: "🥀",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var tulip = {
	keywords: [
		"flowers",
		"plant",
		"nature",
		"summer",
		"spring"
	],
	char: "🌷",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var blossom = {
	keywords: [
		"nature",
		"flowers",
		"yellow"
	],
	char: "🌼",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var cherry_blossom = {
	keywords: [
		"nature",
		"plant",
		"spring",
		"flower"
	],
	char: "🌸",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var bouquet = {
	keywords: [
		"flowers",
		"nature",
		"spring"
	],
	char: "💐",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var mushroom = {
	keywords: [
		"plant",
		"vegetable"
	],
	char: "🍄",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var chestnut = {
	keywords: [
		"food",
		"squirrel"
	],
	char: "🌰",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var jack_o_lantern = {
	keywords: [
		"halloween",
		"light",
		"pumpkin",
		"creepy",
		"fall"
	],
	char: "🎃",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var shell = {
	keywords: [
		"nature",
		"sea",
		"beach"
	],
	char: "🐚",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var spider_web = {
	keywords: [
		"animal",
		"insect",
		"arachnid",
		"silk"
	],
	char: "🕸",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var earth_americas = {
	keywords: [
		"globe",
		"world",
		"USA",
		"international"
	],
	char: "🌎",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var earth_africa = {
	keywords: [
		"globe",
		"world",
		"international"
	],
	char: "🌍",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var earth_asia = {
	keywords: [
		"globe",
		"world",
		"east",
		"international"
	],
	char: "🌏",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var full_moon = {
	keywords: [
		"nature",
		"yellow",
		"twilight",
		"planet",
		"space",
		"night",
		"evening",
		"sleep"
	],
	char: "🌕",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var waning_gibbous_moon = {
	keywords: [
		"nature",
		"twilight",
		"planet",
		"space",
		"night",
		"evening",
		"sleep",
		"waxing_gibbous_moon"
	],
	char: "🌖",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var last_quarter_moon = {
	keywords: [
		"nature",
		"twilight",
		"planet",
		"space",
		"night",
		"evening",
		"sleep"
	],
	char: "🌗",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var waning_crescent_moon = {
	keywords: [
		"nature",
		"twilight",
		"planet",
		"space",
		"night",
		"evening",
		"sleep"
	],
	char: "🌘",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var new_moon = {
	keywords: [
		"nature",
		"twilight",
		"planet",
		"space",
		"night",
		"evening",
		"sleep"
	],
	char: "🌑",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var waxing_crescent_moon = {
	keywords: [
		"nature",
		"twilight",
		"planet",
		"space",
		"night",
		"evening",
		"sleep"
	],
	char: "🌒",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var first_quarter_moon = {
	keywords: [
		"nature",
		"twilight",
		"planet",
		"space",
		"night",
		"evening",
		"sleep"
	],
	char: "🌓",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var waxing_gibbous_moon = {
	keywords: [
		"nature",
		"night",
		"sky",
		"gray",
		"twilight",
		"planet",
		"space",
		"evening",
		"sleep"
	],
	char: "🌔",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var new_moon_with_face = {
	keywords: [
		"nature",
		"twilight",
		"planet",
		"space",
		"night",
		"evening",
		"sleep"
	],
	char: "🌚",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var full_moon_with_face = {
	keywords: [
		"nature",
		"twilight",
		"planet",
		"space",
		"night",
		"evening",
		"sleep"
	],
	char: "🌝",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var first_quarter_moon_with_face = {
	keywords: [
		"nature",
		"twilight",
		"planet",
		"space",
		"night",
		"evening",
		"sleep"
	],
	char: "🌛",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var last_quarter_moon_with_face = {
	keywords: [
		"nature",
		"twilight",
		"planet",
		"space",
		"night",
		"evening",
		"sleep"
	],
	char: "🌜",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var sun_with_face = {
	keywords: [
		"nature",
		"morning",
		"sky"
	],
	char: "🌞",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var crescent_moon = {
	keywords: [
		"night",
		"sleep",
		"sky",
		"evening",
		"magic"
	],
	char: "🌙",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var star = {
	keywords: [
		"night",
		"yellow"
	],
	char: "⭐",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var star2 = {
	keywords: [
		"night",
		"sparkle",
		"awesome",
		"good",
		"magic"
	],
	char: "🌟",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var dizzy = {
	keywords: [
		"star",
		"sparkle",
		"shoot",
		"magic"
	],
	char: "💫",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var sparkles = {
	keywords: [
		"stars",
		"shine",
		"shiny",
		"cool",
		"awesome",
		"good",
		"magic"
	],
	char: "✨",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var comet = {
	keywords: [
		"space"
	],
	char: "☄",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var sunny = {
	keywords: [
		"weather",
		"nature",
		"brightness",
		"summer",
		"beach",
		"spring"
	],
	char: "☀️",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var sun_behind_small_cloud = {
	keywords: [
		"weather"
	],
	char: "🌤",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var partly_sunny = {
	keywords: [
		"weather",
		"nature",
		"cloudy",
		"morning",
		"fall",
		"spring"
	],
	char: "⛅",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var sun_behind_large_cloud = {
	keywords: [
		"weather"
	],
	char: "🌥",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var sun_behind_rain_cloud = {
	keywords: [
		"weather"
	],
	char: "🌦",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var cloud = {
	keywords: [
		"weather",
		"sky"
	],
	char: "☁️",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var cloud_with_rain = {
	keywords: [
		"weather"
	],
	char: "🌧",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var cloud_with_lightning_and_rain = {
	keywords: [
		"weather",
		"lightning"
	],
	char: "⛈",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var cloud_with_lightning = {
	keywords: [
		"weather",
		"thunder"
	],
	char: "🌩",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var zap = {
	keywords: [
		"thunder",
		"weather",
		"lightning bolt",
		"fast"
	],
	char: "⚡",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var fire = {
	keywords: [
		"hot",
		"cook",
		"flame"
	],
	char: "🔥",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var boom = {
	keywords: [
		"bomb",
		"explode",
		"explosion",
		"collision",
		"blown"
	],
	char: "💥",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var snowflake = {
	keywords: [
		"winter",
		"season",
		"cold",
		"weather",
		"christmas",
		"xmas"
	],
	char: "❄️",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var cloud_with_snow = {
	keywords: [
		"weather"
	],
	char: "🌨",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var snowman = {
	keywords: [
		"winter",
		"season",
		"cold",
		"weather",
		"christmas",
		"xmas",
		"frozen",
		"without_snow"
	],
	char: "⛄",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var snowman_with_snow = {
	keywords: [
		"winter",
		"season",
		"cold",
		"weather",
		"christmas",
		"xmas",
		"frozen"
	],
	char: "☃",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var wind_face = {
	keywords: [
		"gust",
		"air"
	],
	char: "🌬",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var dash = {
	keywords: [
		"wind",
		"air",
		"fast",
		"shoo",
		"fart",
		"smoke",
		"puff"
	],
	char: "💨",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var tornado = {
	keywords: [
		"weather",
		"cyclone",
		"twister"
	],
	char: "🌪",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var fog = {
	keywords: [
		"weather"
	],
	char: "🌫",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var open_umbrella = {
	keywords: [
		"weather",
		"spring"
	],
	char: "☂",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var umbrella = {
	keywords: [
		"rainy",
		"weather",
		"spring"
	],
	char: "☔",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var droplet = {
	keywords: [
		"water",
		"drip",
		"faucet",
		"spring"
	],
	char: "💧",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var sweat_drops = {
	keywords: [
		"water",
		"drip",
		"oops"
	],
	char: "💦",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var ocean = {
	keywords: [
		"sea",
		"water",
		"wave",
		"nature",
		"tsunami",
		"disaster"
	],
	char: "🌊",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
};
var green_apple = {
	keywords: [
		"fruit",
		"nature"
	],
	char: "🍏",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var apple = {
	keywords: [
		"fruit",
		"mac",
		"school"
	],
	char: "🍎",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var pear = {
	keywords: [
		"fruit",
		"nature",
		"food"
	],
	char: "🍐",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var tangerine = {
	keywords: [
		"food",
		"fruit",
		"nature",
		"orange"
	],
	char: "🍊",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var lemon = {
	keywords: [
		"fruit",
		"nature"
	],
	char: "🍋",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var banana = {
	keywords: [
		"fruit",
		"food",
		"monkey"
	],
	char: "🍌",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var watermelon = {
	keywords: [
		"fruit",
		"food",
		"picnic",
		"summer"
	],
	char: "🍉",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var grapes = {
	keywords: [
		"fruit",
		"food",
		"wine"
	],
	char: "🍇",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var strawberry = {
	keywords: [
		"fruit",
		"food",
		"nature"
	],
	char: "🍓",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var melon = {
	keywords: [
		"fruit",
		"nature",
		"food"
	],
	char: "🍈",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var cherries = {
	keywords: [
		"food",
		"fruit"
	],
	char: "🍒",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var peach = {
	keywords: [
		"fruit",
		"nature",
		"food"
	],
	char: "🍑",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var pineapple = {
	keywords: [
		"fruit",
		"nature",
		"food"
	],
	char: "🍍",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var coconut = {
	keywords: [
		"fruit",
		"nature",
		"food",
		"palm"
	],
	char: "🥥",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var kiwi_fruit = {
	keywords: [
		"fruit",
		"food"
	],
	char: "🥝",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var mango = {
	keywords: [
		"fruit",
		"food",
		"tropical"
	],
	char: "🥭",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var avocado = {
	keywords: [
		"fruit",
		"food"
	],
	char: "🥑",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var broccoli = {
	keywords: [
		"fruit",
		"food",
		"vegetable"
	],
	char: "🥦",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var tomato = {
	keywords: [
		"fruit",
		"vegetable",
		"nature",
		"food"
	],
	char: "🍅",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var eggplant = {
	keywords: [
		"vegetable",
		"nature",
		"food",
		"aubergine"
	],
	char: "🍆",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var cucumber = {
	keywords: [
		"fruit",
		"food",
		"pickle"
	],
	char: "🥒",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var carrot = {
	keywords: [
		"vegetable",
		"food",
		"orange"
	],
	char: "🥕",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var hot_pepper = {
	keywords: [
		"food",
		"spicy",
		"chilli",
		"chili"
	],
	char: "🌶",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var potato = {
	keywords: [
		"food",
		"tuber",
		"vegatable",
		"starch"
	],
	char: "🥔",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var corn = {
	keywords: [
		"food",
		"vegetable",
		"plant"
	],
	char: "🌽",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var leafy_greens = {
	keywords: [
		"food",
		"vegetable",
		"plant",
		"bok choy",
		"cabbage",
		"kale",
		"lettuce"
	],
	char: "🥬",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var sweet_potato = {
	keywords: [
		"food",
		"nature"
	],
	char: "🍠",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var peanuts = {
	keywords: [
		"food",
		"nut"
	],
	char: "🥜",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var honey_pot = {
	keywords: [
		"bees",
		"sweet",
		"kitchen"
	],
	char: "🍯",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var croissant = {
	keywords: [
		"food",
		"bread",
		"french"
	],
	char: "🥐",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var bread = {
	keywords: [
		"food",
		"wheat",
		"breakfast",
		"toast"
	],
	char: "🍞",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var baguette_bread = {
	keywords: [
		"food",
		"bread",
		"french"
	],
	char: "🥖",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var bagel = {
	keywords: [
		"food",
		"bread",
		"bakery",
		"schmear"
	],
	char: "🥯",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var pretzel = {
	keywords: [
		"food",
		"bread",
		"twisted"
	],
	char: "🥨",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var cheese = {
	keywords: [
		"food",
		"chadder"
	],
	char: "🧀",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var egg = {
	keywords: [
		"food",
		"chicken",
		"breakfast"
	],
	char: "🥚",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var bacon = {
	keywords: [
		"food",
		"breakfast",
		"pork",
		"pig",
		"meat"
	],
	char: "🥓",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var steak = {
	keywords: [
		"food",
		"cow",
		"meat",
		"cut",
		"chop",
		"lambchop",
		"porkchop"
	],
	char: "🥩",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var pancakes = {
	keywords: [
		"food",
		"breakfast",
		"flapjacks",
		"hotcakes"
	],
	char: "🥞",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var poultry_leg = {
	keywords: [
		"food",
		"meat",
		"drumstick",
		"bird",
		"chicken",
		"turkey"
	],
	char: "🍗",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var meat_on_bone = {
	keywords: [
		"good",
		"food",
		"drumstick"
	],
	char: "🍖",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var bone = {
	keywords: [
		"skeleton"
	],
	char: "🦴",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var fried_shrimp = {
	keywords: [
		"food",
		"animal",
		"appetizer",
		"summer"
	],
	char: "🍤",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var fried_egg = {
	keywords: [
		"food",
		"breakfast",
		"kitchen",
		"egg"
	],
	char: "🍳",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var hamburger = {
	keywords: [
		"meat",
		"fast food",
		"beef",
		"cheeseburger",
		"mcdonalds",
		"burger king"
	],
	char: "🍔",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var fries = {
	keywords: [
		"chips",
		"snack",
		"fast food"
	],
	char: "🍟",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var stuffed_flatbread = {
	keywords: [
		"food",
		"flatbread",
		"stuffed",
		"gyro"
	],
	char: "🥙",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var hotdog = {
	keywords: [
		"food",
		"frankfurter"
	],
	char: "🌭",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var pizza = {
	keywords: [
		"food",
		"party"
	],
	char: "🍕",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var sandwich = {
	keywords: [
		"food",
		"lunch",
		"bread"
	],
	char: "🥪",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var canned_food = {
	keywords: [
		"food",
		"soup"
	],
	char: "🥫",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var spaghetti = {
	keywords: [
		"food",
		"italian",
		"noodle"
	],
	char: "🍝",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var taco = {
	keywords: [
		"food",
		"mexican"
	],
	char: "🌮",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var burrito = {
	keywords: [
		"food",
		"mexican"
	],
	char: "🌯",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var green_salad = {
	keywords: [
		"food",
		"healthy",
		"lettuce"
	],
	char: "🥗",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var shallow_pan_of_food = {
	keywords: [
		"food",
		"cooking",
		"casserole",
		"paella"
	],
	char: "🥘",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var ramen = {
	keywords: [
		"food",
		"japanese",
		"noodle",
		"chopsticks"
	],
	char: "🍜",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var stew = {
	keywords: [
		"food",
		"meat",
		"soup"
	],
	char: "🍲",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var fish_cake = {
	keywords: [
		"food",
		"japan",
		"sea",
		"beach",
		"narutomaki",
		"pink",
		"swirl",
		"kamaboko",
		"surimi",
		"ramen"
	],
	char: "🍥",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var fortune_cookie = {
	keywords: [
		"food",
		"prophecy"
	],
	char: "🥠",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var sushi = {
	keywords: [
		"food",
		"fish",
		"japanese",
		"rice"
	],
	char: "🍣",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var bento = {
	keywords: [
		"food",
		"japanese",
		"box"
	],
	char: "🍱",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var curry = {
	keywords: [
		"food",
		"spicy",
		"hot",
		"indian"
	],
	char: "🍛",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var rice_ball = {
	keywords: [
		"food",
		"japanese"
	],
	char: "🍙",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var rice = {
	keywords: [
		"food",
		"china",
		"asian"
	],
	char: "🍚",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var rice_cracker = {
	keywords: [
		"food",
		"japanese"
	],
	char: "🍘",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var oden = {
	keywords: [
		"food",
		"japanese"
	],
	char: "🍢",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var dango = {
	keywords: [
		"food",
		"dessert",
		"sweet",
		"japanese",
		"barbecue",
		"meat"
	],
	char: "🍡",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var shaved_ice = {
	keywords: [
		"hot",
		"dessert",
		"summer"
	],
	char: "🍧",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var ice_cream = {
	keywords: [
		"food",
		"hot",
		"dessert"
	],
	char: "🍨",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var icecream = {
	keywords: [
		"food",
		"hot",
		"dessert",
		"summer"
	],
	char: "🍦",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var pie = {
	keywords: [
		"food",
		"dessert",
		"pastry"
	],
	char: "🥧",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var cake = {
	keywords: [
		"food",
		"dessert"
	],
	char: "🍰",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var cupcake = {
	keywords: [
		"food",
		"dessert",
		"bakery",
		"sweet"
	],
	char: "🧁",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var moon_cake = {
	keywords: [
		"food",
		"autumn"
	],
	char: "🥮",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var birthday = {
	keywords: [
		"food",
		"dessert",
		"cake"
	],
	char: "🎂",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var custard = {
	keywords: [
		"dessert",
		"food"
	],
	char: "🍮",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var candy = {
	keywords: [
		"snack",
		"dessert",
		"sweet",
		"lolly"
	],
	char: "🍬",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var lollipop = {
	keywords: [
		"food",
		"snack",
		"candy",
		"sweet"
	],
	char: "🍭",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var chocolate_bar = {
	keywords: [
		"food",
		"snack",
		"dessert",
		"sweet"
	],
	char: "🍫",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var popcorn = {
	keywords: [
		"food",
		"movie theater",
		"films",
		"snack"
	],
	char: "🍿",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var dumpling = {
	keywords: [
		"food",
		"empanada",
		"pierogi",
		"potsticker"
	],
	char: "🥟",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var doughnut = {
	keywords: [
		"food",
		"dessert",
		"snack",
		"sweet",
		"donut"
	],
	char: "🍩",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var cookie = {
	keywords: [
		"food",
		"snack",
		"oreo",
		"chocolate",
		"sweet",
		"dessert"
	],
	char: "🍪",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var milk_glass = {
	keywords: [
		"beverage",
		"drink",
		"cow"
	],
	char: "🥛",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var beer = {
	keywords: [
		"relax",
		"beverage",
		"drink",
		"drunk",
		"party",
		"pub",
		"summer",
		"alcohol",
		"booze"
	],
	char: "🍺",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var beers = {
	keywords: [
		"relax",
		"beverage",
		"drink",
		"drunk",
		"party",
		"pub",
		"summer",
		"alcohol",
		"booze"
	],
	char: "🍻",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var clinking_glasses = {
	keywords: [
		"beverage",
		"drink",
		"party",
		"alcohol",
		"celebrate",
		"cheers",
		"wine",
		"champagne",
		"toast"
	],
	char: "🥂",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var wine_glass = {
	keywords: [
		"drink",
		"beverage",
		"drunk",
		"alcohol",
		"booze"
	],
	char: "🍷",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var tumbler_glass = {
	keywords: [
		"drink",
		"beverage",
		"drunk",
		"alcohol",
		"liquor",
		"booze",
		"bourbon",
		"scotch",
		"whisky",
		"glass",
		"shot"
	],
	char: "🥃",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var cocktail = {
	keywords: [
		"drink",
		"drunk",
		"alcohol",
		"beverage",
		"booze",
		"mojito"
	],
	char: "🍸",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var tropical_drink = {
	keywords: [
		"beverage",
		"cocktail",
		"summer",
		"beach",
		"alcohol",
		"booze",
		"mojito"
	],
	char: "🍹",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var champagne = {
	keywords: [
		"drink",
		"wine",
		"bottle",
		"celebration"
	],
	char: "🍾",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var sake = {
	keywords: [
		"wine",
		"drink",
		"drunk",
		"beverage",
		"japanese",
		"alcohol",
		"booze"
	],
	char: "🍶",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var tea = {
	keywords: [
		"drink",
		"bowl",
		"breakfast",
		"green",
		"british"
	],
	char: "🍵",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var cup_with_straw = {
	keywords: [
		"drink",
		"soda"
	],
	char: "🥤",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var coffee = {
	keywords: [
		"beverage",
		"caffeine",
		"latte",
		"espresso"
	],
	char: "☕",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var baby_bottle = {
	keywords: [
		"food",
		"container",
		"milk"
	],
	char: "🍼",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var salt = {
	keywords: [
		"condiment",
		"shaker"
	],
	char: "🧂",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var spoon = {
	keywords: [
		"cutlery",
		"kitchen",
		"tableware"
	],
	char: "🥄",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var fork_and_knife = {
	keywords: [
		"cutlery",
		"kitchen"
	],
	char: "🍴",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var plate_with_cutlery = {
	keywords: [
		"food",
		"eat",
		"meal",
		"lunch",
		"dinner",
		"restaurant"
	],
	char: "🍽",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var bowl_with_spoon = {
	keywords: [
		"food",
		"breakfast",
		"cereal",
		"oatmeal",
		"porridge"
	],
	char: "🥣",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var takeout_box = {
	keywords: [
		"food",
		"leftovers"
	],
	char: "🥡",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var chopsticks = {
	keywords: [
		"food"
	],
	char: "🥢",
	fitzpatrick_scale: false,
	category: "food_and_drink"
};
var soccer = {
	keywords: [
		"sports",
		"football"
	],
	char: "⚽",
	fitzpatrick_scale: false,
	category: "activity"
};
var basketball = {
	keywords: [
		"sports",
		"balls",
		"NBA"
	],
	char: "🏀",
	fitzpatrick_scale: false,
	category: "activity"
};
var football = {
	keywords: [
		"sports",
		"balls",
		"NFL"
	],
	char: "🏈",
	fitzpatrick_scale: false,
	category: "activity"
};
var baseball = {
	keywords: [
		"sports",
		"balls"
	],
	char: "⚾",
	fitzpatrick_scale: false,
	category: "activity"
};
var softball = {
	keywords: [
		"sports",
		"balls"
	],
	char: "🥎",
	fitzpatrick_scale: false,
	category: "activity"
};
var tennis = {
	keywords: [
		"sports",
		"balls",
		"green"
	],
	char: "🎾",
	fitzpatrick_scale: false,
	category: "activity"
};
var volleyball = {
	keywords: [
		"sports",
		"balls"
	],
	char: "🏐",
	fitzpatrick_scale: false,
	category: "activity"
};
var rugby_football = {
	keywords: [
		"sports",
		"team"
	],
	char: "🏉",
	fitzpatrick_scale: false,
	category: "activity"
};
var flying_disc = {
	keywords: [
		"sports",
		"frisbee",
		"ultimate"
	],
	char: "🥏",
	fitzpatrick_scale: false,
	category: "activity"
};
var golf = {
	keywords: [
		"sports",
		"business",
		"flag",
		"hole",
		"summer"
	],
	char: "⛳",
	fitzpatrick_scale: false,
	category: "activity"
};
var golfing_woman = {
	keywords: [
		"sports",
		"business",
		"woman",
		"female"
	],
	char: "🏌️‍♀️",
	fitzpatrick_scale: false,
	category: "activity"
};
var golfing_man = {
	keywords: [
		"sports",
		"business"
	],
	char: "🏌",
	fitzpatrick_scale: true,
	category: "activity"
};
var ping_pong = {
	keywords: [
		"sports",
		"pingpong"
	],
	char: "🏓",
	fitzpatrick_scale: false,
	category: "activity"
};
var badminton = {
	keywords: [
		"sports"
	],
	char: "🏸",
	fitzpatrick_scale: false,
	category: "activity"
};
var goal_net = {
	keywords: [
		"sports"
	],
	char: "🥅",
	fitzpatrick_scale: false,
	category: "activity"
};
var ice_hockey = {
	keywords: [
		"sports"
	],
	char: "🏒",
	fitzpatrick_scale: false,
	category: "activity"
};
var field_hockey = {
	keywords: [
		"sports"
	],
	char: "🏑",
	fitzpatrick_scale: false,
	category: "activity"
};
var lacrosse = {
	keywords: [
		"sports",
		"ball",
		"stick"
	],
	char: "🥍",
	fitzpatrick_scale: false,
	category: "activity"
};
var cricket = {
	keywords: [
		"sports"
	],
	char: "🏏",
	fitzpatrick_scale: false,
	category: "activity"
};
var ski = {
	keywords: [
		"sports",
		"winter",
		"cold",
		"snow"
	],
	char: "🎿",
	fitzpatrick_scale: false,
	category: "activity"
};
var skier = {
	keywords: [
		"sports",
		"winter",
		"snow"
	],
	char: "⛷",
	fitzpatrick_scale: false,
	category: "activity"
};
var snowboarder = {
	keywords: [
		"sports",
		"winter"
	],
	char: "🏂",
	fitzpatrick_scale: true,
	category: "activity"
};
var person_fencing = {
	keywords: [
		"sports",
		"fencing",
		"sword"
	],
	char: "🤺",
	fitzpatrick_scale: false,
	category: "activity"
};
var women_wrestling = {
	keywords: [
		"sports",
		"wrestlers"
	],
	char: "🤼‍♀️",
	fitzpatrick_scale: false,
	category: "activity"
};
var men_wrestling = {
	keywords: [
		"sports",
		"wrestlers"
	],
	char: "🤼‍♂️",
	fitzpatrick_scale: false,
	category: "activity"
};
var woman_cartwheeling = {
	keywords: [
		"gymnastics"
	],
	char: "🤸‍♀️",
	fitzpatrick_scale: true,
	category: "activity"
};
var man_cartwheeling = {
	keywords: [
		"gymnastics"
	],
	char: "🤸‍♂️",
	fitzpatrick_scale: true,
	category: "activity"
};
var woman_playing_handball = {
	keywords: [
		"sports"
	],
	char: "🤾‍♀️",
	fitzpatrick_scale: true,
	category: "activity"
};
var man_playing_handball = {
	keywords: [
		"sports"
	],
	char: "🤾‍♂️",
	fitzpatrick_scale: true,
	category: "activity"
};
var ice_skate = {
	keywords: [
		"sports"
	],
	char: "⛸",
	fitzpatrick_scale: false,
	category: "activity"
};
var curling_stone = {
	keywords: [
		"sports"
	],
	char: "🥌",
	fitzpatrick_scale: false,
	category: "activity"
};
var skateboard = {
	keywords: [
		"board"
	],
	char: "🛹",
	fitzpatrick_scale: false,
	category: "activity"
};
var sled = {
	keywords: [
		"sleigh",
		"luge",
		"toboggan"
	],
	char: "🛷",
	fitzpatrick_scale: false,
	category: "activity"
};
var bow_and_arrow = {
	keywords: [
		"sports"
	],
	char: "🏹",
	fitzpatrick_scale: false,
	category: "activity"
};
var fishing_pole_and_fish = {
	keywords: [
		"food",
		"hobby",
		"summer"
	],
	char: "🎣",
	fitzpatrick_scale: false,
	category: "activity"
};
var boxing_glove = {
	keywords: [
		"sports",
		"fighting"
	],
	char: "🥊",
	fitzpatrick_scale: false,
	category: "activity"
};
var martial_arts_uniform = {
	keywords: [
		"judo",
		"karate",
		"taekwondo"
	],
	char: "🥋",
	fitzpatrick_scale: false,
	category: "activity"
};
var rowing_woman = {
	keywords: [
		"sports",
		"hobby",
		"water",
		"ship",
		"woman",
		"female"
	],
	char: "🚣‍♀️",
	fitzpatrick_scale: true,
	category: "activity"
};
var rowing_man = {
	keywords: [
		"sports",
		"hobby",
		"water",
		"ship"
	],
	char: "🚣",
	fitzpatrick_scale: true,
	category: "activity"
};
var climbing_woman = {
	keywords: [
		"sports",
		"hobby",
		"woman",
		"female",
		"rock"
	],
	char: "🧗‍♀️",
	fitzpatrick_scale: true,
	category: "activity"
};
var climbing_man = {
	keywords: [
		"sports",
		"hobby",
		"man",
		"male",
		"rock"
	],
	char: "🧗‍♂️",
	fitzpatrick_scale: true,
	category: "activity"
};
var swimming_woman = {
	keywords: [
		"sports",
		"exercise",
		"human",
		"athlete",
		"water",
		"summer",
		"woman",
		"female"
	],
	char: "🏊‍♀️",
	fitzpatrick_scale: true,
	category: "activity"
};
var swimming_man = {
	keywords: [
		"sports",
		"exercise",
		"human",
		"athlete",
		"water",
		"summer"
	],
	char: "🏊",
	fitzpatrick_scale: true,
	category: "activity"
};
var woman_playing_water_polo = {
	keywords: [
		"sports",
		"pool"
	],
	char: "🤽‍♀️",
	fitzpatrick_scale: true,
	category: "activity"
};
var man_playing_water_polo = {
	keywords: [
		"sports",
		"pool"
	],
	char: "🤽‍♂️",
	fitzpatrick_scale: true,
	category: "activity"
};
var woman_in_lotus_position = {
	keywords: [
		"woman",
		"female",
		"meditation",
		"yoga",
		"serenity",
		"zen",
		"mindfulness"
	],
	char: "🧘‍♀️",
	fitzpatrick_scale: true,
	category: "activity"
};
var man_in_lotus_position = {
	keywords: [
		"man",
		"male",
		"meditation",
		"yoga",
		"serenity",
		"zen",
		"mindfulness"
	],
	char: "🧘‍♂️",
	fitzpatrick_scale: true,
	category: "activity"
};
var surfing_woman = {
	keywords: [
		"sports",
		"ocean",
		"sea",
		"summer",
		"beach",
		"woman",
		"female"
	],
	char: "🏄‍♀️",
	fitzpatrick_scale: true,
	category: "activity"
};
var surfing_man = {
	keywords: [
		"sports",
		"ocean",
		"sea",
		"summer",
		"beach"
	],
	char: "🏄",
	fitzpatrick_scale: true,
	category: "activity"
};
var bath = {
	keywords: [
		"clean",
		"shower",
		"bathroom"
	],
	char: "🛀",
	fitzpatrick_scale: true,
	category: "activity"
};
var basketball_woman = {
	keywords: [
		"sports",
		"human",
		"woman",
		"female"
	],
	char: "⛹️‍♀️",
	fitzpatrick_scale: true,
	category: "activity"
};
var basketball_man = {
	keywords: [
		"sports",
		"human"
	],
	char: "⛹",
	fitzpatrick_scale: true,
	category: "activity"
};
var weight_lifting_woman = {
	keywords: [
		"sports",
		"training",
		"exercise",
		"woman",
		"female"
	],
	char: "🏋️‍♀️",
	fitzpatrick_scale: true,
	category: "activity"
};
var weight_lifting_man = {
	keywords: [
		"sports",
		"training",
		"exercise"
	],
	char: "🏋",
	fitzpatrick_scale: true,
	category: "activity"
};
var biking_woman = {
	keywords: [
		"sports",
		"bike",
		"exercise",
		"hipster",
		"woman",
		"female"
	],
	char: "🚴‍♀️",
	fitzpatrick_scale: true,
	category: "activity"
};
var biking_man = {
	keywords: [
		"sports",
		"bike",
		"exercise",
		"hipster"
	],
	char: "🚴",
	fitzpatrick_scale: true,
	category: "activity"
};
var mountain_biking_woman = {
	keywords: [
		"transportation",
		"sports",
		"human",
		"race",
		"bike",
		"woman",
		"female"
	],
	char: "🚵‍♀️",
	fitzpatrick_scale: true,
	category: "activity"
};
var mountain_biking_man = {
	keywords: [
		"transportation",
		"sports",
		"human",
		"race",
		"bike"
	],
	char: "🚵",
	fitzpatrick_scale: true,
	category: "activity"
};
var horse_racing = {
	keywords: [
		"animal",
		"betting",
		"competition",
		"gambling",
		"luck"
	],
	char: "🏇",
	fitzpatrick_scale: true,
	category: "activity"
};
var business_suit_levitating = {
	keywords: [
		"suit",
		"business",
		"levitate",
		"hover",
		"jump"
	],
	char: "🕴",
	fitzpatrick_scale: true,
	category: "activity"
};
var trophy = {
	keywords: [
		"win",
		"award",
		"contest",
		"place",
		"ftw",
		"ceremony"
	],
	char: "🏆",
	fitzpatrick_scale: false,
	category: "activity"
};
var running_shirt_with_sash = {
	keywords: [
		"play",
		"pageant"
	],
	char: "🎽",
	fitzpatrick_scale: false,
	category: "activity"
};
var medal_sports = {
	keywords: [
		"award",
		"winning"
	],
	char: "🏅",
	fitzpatrick_scale: false,
	category: "activity"
};
var medal_military = {
	keywords: [
		"award",
		"winning",
		"army"
	],
	char: "🎖",
	fitzpatrick_scale: false,
	category: "activity"
};
var reminder_ribbon = {
	keywords: [
		"sports",
		"cause",
		"support",
		"awareness"
	],
	char: "🎗",
	fitzpatrick_scale: false,
	category: "activity"
};
var rosette = {
	keywords: [
		"flower",
		"decoration",
		"military"
	],
	char: "🏵",
	fitzpatrick_scale: false,
	category: "activity"
};
var ticket = {
	keywords: [
		"event",
		"concert",
		"pass"
	],
	char: "🎫",
	fitzpatrick_scale: false,
	category: "activity"
};
var tickets = {
	keywords: [
		"sports",
		"concert",
		"entrance"
	],
	char: "🎟",
	fitzpatrick_scale: false,
	category: "activity"
};
var performing_arts = {
	keywords: [
		"acting",
		"theater",
		"drama"
	],
	char: "🎭",
	fitzpatrick_scale: false,
	category: "activity"
};
var art = {
	keywords: [
		"design",
		"paint",
		"draw",
		"colors"
	],
	char: "🎨",
	fitzpatrick_scale: false,
	category: "activity"
};
var circus_tent = {
	keywords: [
		"festival",
		"carnival",
		"party"
	],
	char: "🎪",
	fitzpatrick_scale: false,
	category: "activity"
};
var woman_juggling = {
	keywords: [
		"juggle",
		"balance",
		"skill",
		"multitask"
	],
	char: "🤹‍♀️",
	fitzpatrick_scale: true,
	category: "activity"
};
var man_juggling = {
	keywords: [
		"juggle",
		"balance",
		"skill",
		"multitask"
	],
	char: "🤹‍♂️",
	fitzpatrick_scale: true,
	category: "activity"
};
var microphone = {
	keywords: [
		"sound",
		"music",
		"PA",
		"sing",
		"talkshow"
	],
	char: "🎤",
	fitzpatrick_scale: false,
	category: "activity"
};
var headphones = {
	keywords: [
		"music",
		"score",
		"gadgets"
	],
	char: "🎧",
	fitzpatrick_scale: false,
	category: "activity"
};
var musical_score = {
	keywords: [
		"treble",
		"clef",
		"compose"
	],
	char: "🎼",
	fitzpatrick_scale: false,
	category: "activity"
};
var musical_keyboard = {
	keywords: [
		"piano",
		"instrument",
		"compose"
	],
	char: "🎹",
	fitzpatrick_scale: false,
	category: "activity"
};
var drum = {
	keywords: [
		"music",
		"instrument",
		"drumsticks",
		"snare"
	],
	char: "🥁",
	fitzpatrick_scale: false,
	category: "activity"
};
var saxophone = {
	keywords: [
		"music",
		"instrument",
		"jazz",
		"blues"
	],
	char: "🎷",
	fitzpatrick_scale: false,
	category: "activity"
};
var trumpet = {
	keywords: [
		"music",
		"brass"
	],
	char: "🎺",
	fitzpatrick_scale: false,
	category: "activity"
};
var guitar = {
	keywords: [
		"music",
		"instrument"
	],
	char: "🎸",
	fitzpatrick_scale: false,
	category: "activity"
};
var violin = {
	keywords: [
		"music",
		"instrument",
		"orchestra",
		"symphony"
	],
	char: "🎻",
	fitzpatrick_scale: false,
	category: "activity"
};
var clapper = {
	keywords: [
		"movie",
		"film",
		"record"
	],
	char: "🎬",
	fitzpatrick_scale: false,
	category: "activity"
};
var video_game = {
	keywords: [
		"play",
		"console",
		"PS4",
		"controller"
	],
	char: "🎮",
	fitzpatrick_scale: false,
	category: "activity"
};
var space_invader = {
	keywords: [
		"game",
		"arcade",
		"play"
	],
	char: "👾",
	fitzpatrick_scale: false,
	category: "activity"
};
var dart = {
	keywords: [
		"game",
		"play",
		"bar",
		"target",
		"bullseye"
	],
	char: "🎯",
	fitzpatrick_scale: false,
	category: "activity"
};
var game_die = {
	keywords: [
		"dice",
		"random",
		"tabletop",
		"play",
		"luck"
	],
	char: "🎲",
	fitzpatrick_scale: false,
	category: "activity"
};
var chess_pawn = {
	keywords: [
		"expendable"
	],
	char: "♟",
	fitzpatrick_scale: false,
	category: "activity"
};
var slot_machine = {
	keywords: [
		"bet",
		"gamble",
		"vegas",
		"fruit machine",
		"luck",
		"casino"
	],
	char: "🎰",
	fitzpatrick_scale: false,
	category: "activity"
};
var jigsaw = {
	keywords: [
		"interlocking",
		"puzzle",
		"piece"
	],
	char: "🧩",
	fitzpatrick_scale: false,
	category: "activity"
};
var bowling = {
	keywords: [
		"sports",
		"fun",
		"play"
	],
	char: "🎳",
	fitzpatrick_scale: false,
	category: "activity"
};
var red_car = {
	keywords: [
		"red",
		"transportation",
		"vehicle"
	],
	char: "🚗",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var taxi = {
	keywords: [
		"uber",
		"vehicle",
		"cars",
		"transportation"
	],
	char: "🚕",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var blue_car = {
	keywords: [
		"transportation",
		"vehicle"
	],
	char: "🚙",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var bus = {
	keywords: [
		"car",
		"vehicle",
		"transportation"
	],
	char: "🚌",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var trolleybus = {
	keywords: [
		"bart",
		"transportation",
		"vehicle"
	],
	char: "🚎",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var racing_car = {
	keywords: [
		"sports",
		"race",
		"fast",
		"formula",
		"f1"
	],
	char: "🏎",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var police_car = {
	keywords: [
		"vehicle",
		"cars",
		"transportation",
		"law",
		"legal",
		"enforcement"
	],
	char: "🚓",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var ambulance = {
	keywords: [
		"health",
		"911",
		"hospital"
	],
	char: "🚑",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var fire_engine = {
	keywords: [
		"transportation",
		"cars",
		"vehicle"
	],
	char: "🚒",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var minibus = {
	keywords: [
		"vehicle",
		"car",
		"transportation"
	],
	char: "🚐",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var truck = {
	keywords: [
		"cars",
		"transportation"
	],
	char: "🚚",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var articulated_lorry = {
	keywords: [
		"vehicle",
		"cars",
		"transportation",
		"express"
	],
	char: "🚛",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var tractor = {
	keywords: [
		"vehicle",
		"car",
		"farming",
		"agriculture"
	],
	char: "🚜",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var kick_scooter = {
	keywords: [
		"vehicle",
		"kick",
		"razor"
	],
	char: "🛴",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var motorcycle = {
	keywords: [
		"race",
		"sports",
		"fast"
	],
	char: "🏍",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var bike = {
	keywords: [
		"sports",
		"bicycle",
		"exercise",
		"hipster"
	],
	char: "🚲",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var motor_scooter = {
	keywords: [
		"vehicle",
		"vespa",
		"sasha"
	],
	char: "🛵",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var rotating_light = {
	keywords: [
		"police",
		"ambulance",
		"911",
		"emergency",
		"alert",
		"error",
		"pinged",
		"law",
		"legal"
	],
	char: "🚨",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var oncoming_police_car = {
	keywords: [
		"vehicle",
		"law",
		"legal",
		"enforcement",
		"911"
	],
	char: "🚔",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var oncoming_bus = {
	keywords: [
		"vehicle",
		"transportation"
	],
	char: "🚍",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var oncoming_automobile = {
	keywords: [
		"car",
		"vehicle",
		"transportation"
	],
	char: "🚘",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var oncoming_taxi = {
	keywords: [
		"vehicle",
		"cars",
		"uber"
	],
	char: "🚖",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var aerial_tramway = {
	keywords: [
		"transportation",
		"vehicle",
		"ski"
	],
	char: "🚡",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var mountain_cableway = {
	keywords: [
		"transportation",
		"vehicle",
		"ski"
	],
	char: "🚠",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var suspension_railway = {
	keywords: [
		"vehicle",
		"transportation"
	],
	char: "🚟",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var railway_car = {
	keywords: [
		"transportation",
		"vehicle"
	],
	char: "🚃",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var train = {
	keywords: [
		"transportation",
		"vehicle",
		"carriage",
		"public",
		"travel"
	],
	char: "🚋",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var monorail = {
	keywords: [
		"transportation",
		"vehicle"
	],
	char: "🚝",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var bullettrain_side = {
	keywords: [
		"transportation",
		"vehicle"
	],
	char: "🚄",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var bullettrain_front = {
	keywords: [
		"transportation",
		"vehicle",
		"speed",
		"fast",
		"public",
		"travel"
	],
	char: "🚅",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var light_rail = {
	keywords: [
		"transportation",
		"vehicle"
	],
	char: "🚈",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var mountain_railway = {
	keywords: [
		"transportation",
		"vehicle"
	],
	char: "🚞",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var steam_locomotive = {
	keywords: [
		"transportation",
		"vehicle",
		"train"
	],
	char: "🚂",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var train2 = {
	keywords: [
		"transportation",
		"vehicle"
	],
	char: "🚆",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var metro = {
	keywords: [
		"transportation",
		"blue-square",
		"mrt",
		"underground",
		"tube"
	],
	char: "🚇",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var tram = {
	keywords: [
		"transportation",
		"vehicle"
	],
	char: "🚊",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var station = {
	keywords: [
		"transportation",
		"vehicle",
		"public"
	],
	char: "🚉",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var flying_saucer = {
	keywords: [
		"transportation",
		"vehicle",
		"ufo"
	],
	char: "🛸",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var helicopter = {
	keywords: [
		"transportation",
		"vehicle",
		"fly"
	],
	char: "🚁",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var small_airplane = {
	keywords: [
		"flight",
		"transportation",
		"fly",
		"vehicle"
	],
	char: "🛩",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var airplane = {
	keywords: [
		"vehicle",
		"transportation",
		"flight",
		"fly"
	],
	char: "✈️",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var flight_departure = {
	keywords: [
		"airport",
		"flight",
		"landing"
	],
	char: "🛫",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var flight_arrival = {
	keywords: [
		"airport",
		"flight",
		"boarding"
	],
	char: "🛬",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var sailboat = {
	keywords: [
		"ship",
		"summer",
		"transportation",
		"water",
		"sailing"
	],
	char: "⛵",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var motor_boat = {
	keywords: [
		"ship"
	],
	char: "🛥",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var speedboat = {
	keywords: [
		"ship",
		"transportation",
		"vehicle",
		"summer"
	],
	char: "🚤",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var ferry = {
	keywords: [
		"boat",
		"ship",
		"yacht"
	],
	char: "⛴",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var passenger_ship = {
	keywords: [
		"yacht",
		"cruise",
		"ferry"
	],
	char: "🛳",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var rocket = {
	keywords: [
		"launch",
		"ship",
		"staffmode",
		"NASA",
		"outer space",
		"outer_space",
		"fly"
	],
	char: "🚀",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var artificial_satellite = {
	keywords: [
		"communication",
		"gps",
		"orbit",
		"spaceflight",
		"NASA",
		"ISS"
	],
	char: "🛰",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var seat = {
	keywords: [
		"sit",
		"airplane",
		"transport",
		"bus",
		"flight",
		"fly"
	],
	char: "💺",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var canoe = {
	keywords: [
		"boat",
		"paddle",
		"water",
		"ship"
	],
	char: "🛶",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var anchor = {
	keywords: [
		"ship",
		"ferry",
		"sea",
		"boat"
	],
	char: "⚓",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var construction = {
	keywords: [
		"wip",
		"progress",
		"caution",
		"warning"
	],
	char: "🚧",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var fuelpump = {
	keywords: [
		"gas station",
		"petroleum"
	],
	char: "⛽",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var busstop = {
	keywords: [
		"transportation",
		"wait"
	],
	char: "🚏",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var vertical_traffic_light = {
	keywords: [
		"transportation",
		"driving"
	],
	char: "🚦",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var traffic_light = {
	keywords: [
		"transportation",
		"signal"
	],
	char: "🚥",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var checkered_flag = {
	keywords: [
		"contest",
		"finishline",
		"race",
		"gokart"
	],
	char: "🏁",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var ship = {
	keywords: [
		"transportation",
		"titanic",
		"deploy"
	],
	char: "🚢",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var ferris_wheel = {
	keywords: [
		"photo",
		"carnival",
		"londoneye"
	],
	char: "🎡",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var roller_coaster = {
	keywords: [
		"carnival",
		"playground",
		"photo",
		"fun"
	],
	char: "🎢",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var carousel_horse = {
	keywords: [
		"photo",
		"carnival"
	],
	char: "🎠",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var building_construction = {
	keywords: [
		"wip",
		"working",
		"progress"
	],
	char: "🏗",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var foggy = {
	keywords: [
		"photo",
		"mountain"
	],
	char: "🌁",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var tokyo_tower = {
	keywords: [
		"photo",
		"japanese"
	],
	char: "🗼",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var factory = {
	keywords: [
		"building",
		"industry",
		"pollution",
		"smoke"
	],
	char: "🏭",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var fountain = {
	keywords: [
		"photo",
		"summer",
		"water",
		"fresh"
	],
	char: "⛲",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var rice_scene = {
	keywords: [
		"photo",
		"japan",
		"asia",
		"tsukimi"
	],
	char: "🎑",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var mountain = {
	keywords: [
		"photo",
		"nature",
		"environment"
	],
	char: "⛰",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var mountain_snow = {
	keywords: [
		"photo",
		"nature",
		"environment",
		"winter",
		"cold"
	],
	char: "🏔",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var mount_fuji = {
	keywords: [
		"photo",
		"mountain",
		"nature",
		"japanese"
	],
	char: "🗻",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var volcano = {
	keywords: [
		"photo",
		"nature",
		"disaster"
	],
	char: "🌋",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var japan = {
	keywords: [
		"nation",
		"country",
		"japanese",
		"asia"
	],
	char: "🗾",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var camping = {
	keywords: [
		"photo",
		"outdoors",
		"tent"
	],
	char: "🏕",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var tent = {
	keywords: [
		"photo",
		"camping",
		"outdoors"
	],
	char: "⛺",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var national_park = {
	keywords: [
		"photo",
		"environment",
		"nature"
	],
	char: "🏞",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var motorway = {
	keywords: [
		"road",
		"cupertino",
		"interstate",
		"highway"
	],
	char: "🛣",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var railway_track = {
	keywords: [
		"train",
		"transportation"
	],
	char: "🛤",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var sunrise = {
	keywords: [
		"morning",
		"view",
		"vacation",
		"photo"
	],
	char: "🌅",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var sunrise_over_mountains = {
	keywords: [
		"view",
		"vacation",
		"photo"
	],
	char: "🌄",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var desert = {
	keywords: [
		"photo",
		"warm",
		"saharah"
	],
	char: "🏜",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var beach_umbrella = {
	keywords: [
		"weather",
		"summer",
		"sunny",
		"sand",
		"mojito"
	],
	char: "🏖",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var desert_island = {
	keywords: [
		"photo",
		"tropical",
		"mojito"
	],
	char: "🏝",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var city_sunrise = {
	keywords: [
		"photo",
		"good morning",
		"dawn"
	],
	char: "🌇",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var city_sunset = {
	keywords: [
		"photo",
		"evening",
		"sky",
		"buildings"
	],
	char: "🌆",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var cityscape = {
	keywords: [
		"photo",
		"night life",
		"urban"
	],
	char: "🏙",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var night_with_stars = {
	keywords: [
		"evening",
		"city",
		"downtown"
	],
	char: "🌃",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var bridge_at_night = {
	keywords: [
		"photo",
		"sanfrancisco"
	],
	char: "🌉",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var milky_way = {
	keywords: [
		"photo",
		"space",
		"stars"
	],
	char: "🌌",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var stars = {
	keywords: [
		"night",
		"photo"
	],
	char: "🌠",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var sparkler = {
	keywords: [
		"stars",
		"night",
		"shine"
	],
	char: "🎇",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var fireworks = {
	keywords: [
		"photo",
		"festival",
		"carnival",
		"congratulations"
	],
	char: "🎆",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var rainbow = {
	keywords: [
		"nature",
		"happy",
		"unicorn_face",
		"photo",
		"sky",
		"spring"
	],
	char: "🌈",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var houses = {
	keywords: [
		"buildings",
		"photo"
	],
	char: "🏘",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var european_castle = {
	keywords: [
		"building",
		"royalty",
		"history"
	],
	char: "🏰",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var japanese_castle = {
	keywords: [
		"photo",
		"building"
	],
	char: "🏯",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var stadium = {
	keywords: [
		"photo",
		"place",
		"sports",
		"concert",
		"venue"
	],
	char: "🏟",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var statue_of_liberty = {
	keywords: [
		"american",
		"newyork"
	],
	char: "🗽",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var house = {
	keywords: [
		"building",
		"home"
	],
	char: "🏠",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var house_with_garden = {
	keywords: [
		"home",
		"plant",
		"nature"
	],
	char: "🏡",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var derelict_house = {
	keywords: [
		"abandon",
		"evict",
		"broken",
		"building"
	],
	char: "🏚",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var office = {
	keywords: [
		"building",
		"bureau",
		"work"
	],
	char: "🏢",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var department_store = {
	keywords: [
		"building",
		"shopping",
		"mall"
	],
	char: "🏬",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var post_office = {
	keywords: [
		"building",
		"envelope",
		"communication"
	],
	char: "🏣",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var european_post_office = {
	keywords: [
		"building",
		"email"
	],
	char: "🏤",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var hospital = {
	keywords: [
		"building",
		"health",
		"surgery",
		"doctor"
	],
	char: "🏥",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var bank = {
	keywords: [
		"building",
		"money",
		"sales",
		"cash",
		"business",
		"enterprise"
	],
	char: "🏦",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var hotel = {
	keywords: [
		"building",
		"accomodation",
		"checkin"
	],
	char: "🏨",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var convenience_store = {
	keywords: [
		"building",
		"shopping",
		"groceries"
	],
	char: "🏪",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var school = {
	keywords: [
		"building",
		"student",
		"education",
		"learn",
		"teach"
	],
	char: "🏫",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var love_hotel = {
	keywords: [
		"like",
		"affection",
		"dating"
	],
	char: "🏩",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var wedding = {
	keywords: [
		"love",
		"like",
		"affection",
		"couple",
		"marriage",
		"bride",
		"groom"
	],
	char: "💒",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var classical_building = {
	keywords: [
		"art",
		"culture",
		"history"
	],
	char: "🏛",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var church = {
	keywords: [
		"building",
		"religion",
		"christ"
	],
	char: "⛪",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var mosque = {
	keywords: [
		"islam",
		"worship",
		"minaret"
	],
	char: "🕌",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var synagogue = {
	keywords: [
		"judaism",
		"worship",
		"temple",
		"jewish"
	],
	char: "🕍",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var kaaba = {
	keywords: [
		"mecca",
		"mosque",
		"islam"
	],
	char: "🕋",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var shinto_shrine = {
	keywords: [
		"temple",
		"japan",
		"kyoto"
	],
	char: "⛩",
	fitzpatrick_scale: false,
	category: "travel_and_places"
};
var watch = {
	keywords: [
		"time",
		"accessories"
	],
	char: "⌚",
	fitzpatrick_scale: false,
	category: "objects"
};
var iphone = {
	keywords: [
		"technology",
		"apple",
		"gadgets",
		"dial"
	],
	char: "📱",
	fitzpatrick_scale: false,
	category: "objects"
};
var calling = {
	keywords: [
		"iphone",
		"incoming"
	],
	char: "📲",
	fitzpatrick_scale: false,
	category: "objects"
};
var computer = {
	keywords: [
		"technology",
		"laptop",
		"screen",
		"display",
		"monitor"
	],
	char: "💻",
	fitzpatrick_scale: false,
	category: "objects"
};
var keyboard = {
	keywords: [
		"technology",
		"computer",
		"type",
		"input",
		"text"
	],
	char: "⌨",
	fitzpatrick_scale: false,
	category: "objects"
};
var desktop_computer = {
	keywords: [
		"technology",
		"computing",
		"screen"
	],
	char: "🖥",
	fitzpatrick_scale: false,
	category: "objects"
};
var printer = {
	keywords: [
		"paper",
		"ink"
	],
	char: "🖨",
	fitzpatrick_scale: false,
	category: "objects"
};
var computer_mouse = {
	keywords: [
		"click"
	],
	char: "🖱",
	fitzpatrick_scale: false,
	category: "objects"
};
var trackball = {
	keywords: [
		"technology",
		"trackpad"
	],
	char: "🖲",
	fitzpatrick_scale: false,
	category: "objects"
};
var joystick = {
	keywords: [
		"game",
		"play"
	],
	char: "🕹",
	fitzpatrick_scale: false,
	category: "objects"
};
var clamp = {
	keywords: [
		"tool"
	],
	char: "🗜",
	fitzpatrick_scale: false,
	category: "objects"
};
var minidisc = {
	keywords: [
		"technology",
		"record",
		"data",
		"disk",
		"90s"
	],
	char: "💽",
	fitzpatrick_scale: false,
	category: "objects"
};
var floppy_disk = {
	keywords: [
		"oldschool",
		"technology",
		"save",
		"90s",
		"80s"
	],
	char: "💾",
	fitzpatrick_scale: false,
	category: "objects"
};
var cd = {
	keywords: [
		"technology",
		"dvd",
		"disk",
		"disc",
		"90s"
	],
	char: "💿",
	fitzpatrick_scale: false,
	category: "objects"
};
var dvd = {
	keywords: [
		"cd",
		"disk",
		"disc"
	],
	char: "📀",
	fitzpatrick_scale: false,
	category: "objects"
};
var vhs = {
	keywords: [
		"record",
		"video",
		"oldschool",
		"90s",
		"80s"
	],
	char: "📼",
	fitzpatrick_scale: false,
	category: "objects"
};
var camera = {
	keywords: [
		"gadgets",
		"photography"
	],
	char: "📷",
	fitzpatrick_scale: false,
	category: "objects"
};
var camera_flash = {
	keywords: [
		"photography",
		"gadgets"
	],
	char: "📸",
	fitzpatrick_scale: false,
	category: "objects"
};
var video_camera = {
	keywords: [
		"film",
		"record"
	],
	char: "📹",
	fitzpatrick_scale: false,
	category: "objects"
};
var movie_camera = {
	keywords: [
		"film",
		"record"
	],
	char: "🎥",
	fitzpatrick_scale: false,
	category: "objects"
};
var film_projector = {
	keywords: [
		"video",
		"tape",
		"record",
		"movie"
	],
	char: "📽",
	fitzpatrick_scale: false,
	category: "objects"
};
var film_strip = {
	keywords: [
		"movie"
	],
	char: "🎞",
	fitzpatrick_scale: false,
	category: "objects"
};
var telephone_receiver = {
	keywords: [
		"technology",
		"communication",
		"dial"
	],
	char: "📞",
	fitzpatrick_scale: false,
	category: "objects"
};
var phone = {
	keywords: [
		"technology",
		"communication",
		"dial",
		"telephone"
	],
	char: "☎️",
	fitzpatrick_scale: false,
	category: "objects"
};
var pager = {
	keywords: [
		"bbcall",
		"oldschool",
		"90s"
	],
	char: "📟",
	fitzpatrick_scale: false,
	category: "objects"
};
var fax = {
	keywords: [
		"communication",
		"technology"
	],
	char: "📠",
	fitzpatrick_scale: false,
	category: "objects"
};
var tv = {
	keywords: [
		"technology",
		"program",
		"oldschool",
		"show",
		"television"
	],
	char: "📺",
	fitzpatrick_scale: false,
	category: "objects"
};
var radio = {
	keywords: [
		"communication",
		"music",
		"podcast",
		"program"
	],
	char: "📻",
	fitzpatrick_scale: false,
	category: "objects"
};
var studio_microphone = {
	keywords: [
		"sing",
		"recording",
		"artist",
		"talkshow"
	],
	char: "🎙",
	fitzpatrick_scale: false,
	category: "objects"
};
var level_slider = {
	keywords: [
		"scale"
	],
	char: "🎚",
	fitzpatrick_scale: false,
	category: "objects"
};
var control_knobs = {
	keywords: [
		"dial"
	],
	char: "🎛",
	fitzpatrick_scale: false,
	category: "objects"
};
var compass = {
	keywords: [
		"magnetic",
		"navigation",
		"orienteering"
	],
	char: "🧭",
	fitzpatrick_scale: false,
	category: "objects"
};
var stopwatch = {
	keywords: [
		"time",
		"deadline"
	],
	char: "⏱",
	fitzpatrick_scale: false,
	category: "objects"
};
var timer_clock = {
	keywords: [
		"alarm"
	],
	char: "⏲",
	fitzpatrick_scale: false,
	category: "objects"
};
var alarm_clock = {
	keywords: [
		"time",
		"wake"
	],
	char: "⏰",
	fitzpatrick_scale: false,
	category: "objects"
};
var mantelpiece_clock = {
	keywords: [
		"time"
	],
	char: "🕰",
	fitzpatrick_scale: false,
	category: "objects"
};
var hourglass_flowing_sand = {
	keywords: [
		"oldschool",
		"time",
		"countdown"
	],
	char: "⏳",
	fitzpatrick_scale: false,
	category: "objects"
};
var hourglass = {
	keywords: [
		"time",
		"clock",
		"oldschool",
		"limit",
		"exam",
		"quiz",
		"test"
	],
	char: "⌛",
	fitzpatrick_scale: false,
	category: "objects"
};
var satellite = {
	keywords: [
		"communication",
		"future",
		"radio",
		"space"
	],
	char: "📡",
	fitzpatrick_scale: false,
	category: "objects"
};
var battery = {
	keywords: [
		"power",
		"energy",
		"sustain"
	],
	char: "🔋",
	fitzpatrick_scale: false,
	category: "objects"
};
var electric_plug = {
	keywords: [
		"charger",
		"power"
	],
	char: "🔌",
	fitzpatrick_scale: false,
	category: "objects"
};
var bulb = {
	keywords: [
		"light",
		"electricity",
		"idea"
	],
	char: "💡",
	fitzpatrick_scale: false,
	category: "objects"
};
var flashlight = {
	keywords: [
		"dark",
		"camping",
		"sight",
		"night"
	],
	char: "🔦",
	fitzpatrick_scale: false,
	category: "objects"
};
var candle = {
	keywords: [
		"fire",
		"wax"
	],
	char: "🕯",
	fitzpatrick_scale: false,
	category: "objects"
};
var fire_extinguisher = {
	keywords: [
		"quench"
	],
	char: "🧯",
	fitzpatrick_scale: false,
	category: "objects"
};
var wastebasket = {
	keywords: [
		"bin",
		"trash",
		"rubbish",
		"garbage",
		"toss"
	],
	char: "🗑",
	fitzpatrick_scale: false,
	category: "objects"
};
var oil_drum = {
	keywords: [
		"barrell"
	],
	char: "🛢",
	fitzpatrick_scale: false,
	category: "objects"
};
var money_with_wings = {
	keywords: [
		"dollar",
		"bills",
		"payment",
		"sale"
	],
	char: "💸",
	fitzpatrick_scale: false,
	category: "objects"
};
var dollar = {
	keywords: [
		"money",
		"sales",
		"bill",
		"currency"
	],
	char: "💵",
	fitzpatrick_scale: false,
	category: "objects"
};
var yen = {
	keywords: [
		"money",
		"sales",
		"japanese",
		"dollar",
		"currency"
	],
	char: "💴",
	fitzpatrick_scale: false,
	category: "objects"
};
var euro = {
	keywords: [
		"money",
		"sales",
		"dollar",
		"currency"
	],
	char: "💶",
	fitzpatrick_scale: false,
	category: "objects"
};
var pound = {
	keywords: [
		"british",
		"sterling",
		"money",
		"sales",
		"bills",
		"uk",
		"england",
		"currency"
	],
	char: "💷",
	fitzpatrick_scale: false,
	category: "objects"
};
var moneybag = {
	keywords: [
		"dollar",
		"payment",
		"coins",
		"sale"
	],
	char: "💰",
	fitzpatrick_scale: false,
	category: "objects"
};
var credit_card = {
	keywords: [
		"money",
		"sales",
		"dollar",
		"bill",
		"payment",
		"shopping"
	],
	char: "💳",
	fitzpatrick_scale: false,
	category: "objects"
};
var gem = {
	keywords: [
		"blue",
		"ruby",
		"diamond",
		"jewelry"
	],
	char: "💎",
	fitzpatrick_scale: false,
	category: "objects"
};
var balance_scale = {
	keywords: [
		"law",
		"fairness",
		"weight"
	],
	char: "⚖",
	fitzpatrick_scale: false,
	category: "objects"
};
var toolbox = {
	keywords: [
		"tools",
		"diy",
		"fix",
		"maintainer",
		"mechanic"
	],
	char: "🧰",
	fitzpatrick_scale: false,
	category: "objects"
};
var wrench = {
	keywords: [
		"tools",
		"diy",
		"ikea",
		"fix",
		"maintainer"
	],
	char: "🔧",
	fitzpatrick_scale: false,
	category: "objects"
};
var hammer = {
	keywords: [
		"tools",
		"build",
		"create"
	],
	char: "🔨",
	fitzpatrick_scale: false,
	category: "objects"
};
var hammer_and_pick = {
	keywords: [
		"tools",
		"build",
		"create"
	],
	char: "⚒",
	fitzpatrick_scale: false,
	category: "objects"
};
var hammer_and_wrench = {
	keywords: [
		"tools",
		"build",
		"create"
	],
	char: "🛠",
	fitzpatrick_scale: false,
	category: "objects"
};
var pick = {
	keywords: [
		"tools",
		"dig"
	],
	char: "⛏",
	fitzpatrick_scale: false,
	category: "objects"
};
var nut_and_bolt = {
	keywords: [
		"handy",
		"tools",
		"fix"
	],
	char: "🔩",
	fitzpatrick_scale: false,
	category: "objects"
};
var gear = {
	keywords: [
		"cog"
	],
	char: "⚙",
	fitzpatrick_scale: false,
	category: "objects"
};
var brick = {
	keywords: [
		"bricks"
	],
	char: "🧱",
	fitzpatrick_scale: false,
	category: "objects"
};
var chains = {
	keywords: [
		"lock",
		"arrest"
	],
	char: "⛓",
	fitzpatrick_scale: false,
	category: "objects"
};
var magnet = {
	keywords: [
		"attraction",
		"magnetic"
	],
	char: "🧲",
	fitzpatrick_scale: false,
	category: "objects"
};
var gun = {
	keywords: [
		"violence",
		"weapon",
		"pistol",
		"revolver"
	],
	char: "🔫",
	fitzpatrick_scale: false,
	category: "objects"
};
var bomb = {
	keywords: [
		"boom",
		"explode",
		"explosion",
		"terrorism"
	],
	char: "💣",
	fitzpatrick_scale: false,
	category: "objects"
};
var firecracker = {
	keywords: [
		"dynamite",
		"boom",
		"explode",
		"explosion",
		"explosive"
	],
	char: "🧨",
	fitzpatrick_scale: false,
	category: "objects"
};
var hocho = {
	keywords: [
		"knife",
		"blade",
		"cutlery",
		"kitchen",
		"weapon"
	],
	char: "🔪",
	fitzpatrick_scale: false,
	category: "objects"
};
var dagger = {
	keywords: [
		"weapon"
	],
	char: "🗡",
	fitzpatrick_scale: false,
	category: "objects"
};
var crossed_swords = {
	keywords: [
		"weapon"
	],
	char: "⚔",
	fitzpatrick_scale: false,
	category: "objects"
};
var shield = {
	keywords: [
		"protection",
		"security"
	],
	char: "🛡",
	fitzpatrick_scale: false,
	category: "objects"
};
var smoking = {
	keywords: [
		"kills",
		"tobacco",
		"cigarette",
		"joint",
		"smoke"
	],
	char: "🚬",
	fitzpatrick_scale: false,
	category: "objects"
};
var skull_and_crossbones = {
	keywords: [
		"poison",
		"danger",
		"deadly",
		"scary",
		"death",
		"pirate",
		"evil"
	],
	char: "☠",
	fitzpatrick_scale: false,
	category: "objects"
};
var coffin = {
	keywords: [
		"vampire",
		"dead",
		"die",
		"death",
		"rip",
		"graveyard",
		"cemetery",
		"casket",
		"funeral",
		"box"
	],
	char: "⚰",
	fitzpatrick_scale: false,
	category: "objects"
};
var funeral_urn = {
	keywords: [
		"dead",
		"die",
		"death",
		"rip",
		"ashes"
	],
	char: "⚱",
	fitzpatrick_scale: false,
	category: "objects"
};
var amphora = {
	keywords: [
		"vase",
		"jar"
	],
	char: "🏺",
	fitzpatrick_scale: false,
	category: "objects"
};
var crystal_ball = {
	keywords: [
		"disco",
		"party",
		"magic",
		"circus",
		"fortune_teller"
	],
	char: "🔮",
	fitzpatrick_scale: false,
	category: "objects"
};
var prayer_beads = {
	keywords: [
		"dhikr",
		"religious"
	],
	char: "📿",
	fitzpatrick_scale: false,
	category: "objects"
};
var nazar_amulet = {
	keywords: [
		"bead",
		"charm"
	],
	char: "🧿",
	fitzpatrick_scale: false,
	category: "objects"
};
var barber = {
	keywords: [
		"hair",
		"salon",
		"style"
	],
	char: "💈",
	fitzpatrick_scale: false,
	category: "objects"
};
var alembic = {
	keywords: [
		"distilling",
		"science",
		"experiment",
		"chemistry"
	],
	char: "⚗",
	fitzpatrick_scale: false,
	category: "objects"
};
var telescope = {
	keywords: [
		"stars",
		"space",
		"zoom",
		"science",
		"astronomy"
	],
	char: "🔭",
	fitzpatrick_scale: false,
	category: "objects"
};
var microscope = {
	keywords: [
		"laboratory",
		"experiment",
		"zoomin",
		"science",
		"study"
	],
	char: "🔬",
	fitzpatrick_scale: false,
	category: "objects"
};
var hole = {
	keywords: [
		"embarrassing"
	],
	char: "🕳",
	fitzpatrick_scale: false,
	category: "objects"
};
var pill = {
	keywords: [
		"health",
		"medicine",
		"doctor",
		"pharmacy",
		"drug"
	],
	char: "💊",
	fitzpatrick_scale: false,
	category: "objects"
};
var syringe = {
	keywords: [
		"health",
		"hospital",
		"drugs",
		"blood",
		"medicine",
		"needle",
		"doctor",
		"nurse"
	],
	char: "💉",
	fitzpatrick_scale: false,
	category: "objects"
};
var dna = {
	keywords: [
		"biologist",
		"genetics",
		"life"
	],
	char: "🧬",
	fitzpatrick_scale: false,
	category: "objects"
};
var microbe = {
	keywords: [
		"amoeba",
		"bacteria",
		"germs"
	],
	char: "🦠",
	fitzpatrick_scale: false,
	category: "objects"
};
var petri_dish = {
	keywords: [
		"bacteria",
		"biology",
		"culture",
		"lab"
	],
	char: "🧫",
	fitzpatrick_scale: false,
	category: "objects"
};
var test_tube = {
	keywords: [
		"chemistry",
		"experiment",
		"lab",
		"science"
	],
	char: "🧪",
	fitzpatrick_scale: false,
	category: "objects"
};
var thermometer = {
	keywords: [
		"weather",
		"temperature",
		"hot",
		"cold"
	],
	char: "🌡",
	fitzpatrick_scale: false,
	category: "objects"
};
var broom = {
	keywords: [
		"cleaning",
		"sweeping",
		"witch"
	],
	char: "🧹",
	fitzpatrick_scale: false,
	category: "objects"
};
var basket = {
	keywords: [
		"laundry"
	],
	char: "🧺",
	fitzpatrick_scale: false,
	category: "objects"
};
var toilet_paper = {
	keywords: [
		"roll"
	],
	char: "🧻",
	fitzpatrick_scale: false,
	category: "objects"
};
var label = {
	keywords: [
		"sale",
		"tag"
	],
	char: "🏷",
	fitzpatrick_scale: false,
	category: "objects"
};
var bookmark = {
	keywords: [
		"favorite",
		"label",
		"save"
	],
	char: "🔖",
	fitzpatrick_scale: false,
	category: "objects"
};
var toilet = {
	keywords: [
		"restroom",
		"wc",
		"washroom",
		"bathroom",
		"potty"
	],
	char: "🚽",
	fitzpatrick_scale: false,
	category: "objects"
};
var shower = {
	keywords: [
		"clean",
		"water",
		"bathroom"
	],
	char: "🚿",
	fitzpatrick_scale: false,
	category: "objects"
};
var bathtub = {
	keywords: [
		"clean",
		"shower",
		"bathroom"
	],
	char: "🛁",
	fitzpatrick_scale: false,
	category: "objects"
};
var soap = {
	keywords: [
		"bar",
		"bathing",
		"cleaning",
		"lather"
	],
	char: "🧼",
	fitzpatrick_scale: false,
	category: "objects"
};
var sponge = {
	keywords: [
		"absorbing",
		"cleaning",
		"porous"
	],
	char: "🧽",
	fitzpatrick_scale: false,
	category: "objects"
};
var lotion_bottle = {
	keywords: [
		"moisturizer",
		"sunscreen"
	],
	char: "🧴",
	fitzpatrick_scale: false,
	category: "objects"
};
var key = {
	keywords: [
		"lock",
		"door",
		"password"
	],
	char: "🔑",
	fitzpatrick_scale: false,
	category: "objects"
};
var old_key = {
	keywords: [
		"lock",
		"door",
		"password"
	],
	char: "🗝",
	fitzpatrick_scale: false,
	category: "objects"
};
var couch_and_lamp = {
	keywords: [
		"read",
		"chill"
	],
	char: "🛋",
	fitzpatrick_scale: false,
	category: "objects"
};
var sleeping_bed = {
	keywords: [
		"bed",
		"rest"
	],
	char: "🛌",
	fitzpatrick_scale: true,
	category: "objects"
};
var bed = {
	keywords: [
		"sleep",
		"rest"
	],
	char: "🛏",
	fitzpatrick_scale: false,
	category: "objects"
};
var door = {
	keywords: [
		"house",
		"entry",
		"exit"
	],
	char: "🚪",
	fitzpatrick_scale: false,
	category: "objects"
};
var bellhop_bell = {
	keywords: [
		"service"
	],
	char: "🛎",
	fitzpatrick_scale: false,
	category: "objects"
};
var teddy_bear = {
	keywords: [
		"plush",
		"stuffed"
	],
	char: "🧸",
	fitzpatrick_scale: false,
	category: "objects"
};
var framed_picture = {
	keywords: [
		"photography"
	],
	char: "🖼",
	fitzpatrick_scale: false,
	category: "objects"
};
var world_map = {
	keywords: [
		"location",
		"direction"
	],
	char: "🗺",
	fitzpatrick_scale: false,
	category: "objects"
};
var parasol_on_ground = {
	keywords: [
		"weather",
		"summer"
	],
	char: "⛱",
	fitzpatrick_scale: false,
	category: "objects"
};
var moyai = {
	keywords: [
		"rock",
		"easter island",
		"moai"
	],
	char: "🗿",
	fitzpatrick_scale: false,
	category: "objects"
};
var shopping = {
	keywords: [
		"mall",
		"buy",
		"purchase"
	],
	char: "🛍",
	fitzpatrick_scale: false,
	category: "objects"
};
var shopping_cart = {
	keywords: [
		"trolley"
	],
	char: "🛒",
	fitzpatrick_scale: false,
	category: "objects"
};
var balloon = {
	keywords: [
		"party",
		"celebration",
		"birthday",
		"circus"
	],
	char: "🎈",
	fitzpatrick_scale: false,
	category: "objects"
};
var flags = {
	keywords: [
		"fish",
		"japanese",
		"koinobori",
		"carp",
		"banner"
	],
	char: "🎏",
	fitzpatrick_scale: false,
	category: "objects"
};
var ribbon = {
	keywords: [
		"decoration",
		"pink",
		"girl",
		"bowtie"
	],
	char: "🎀",
	fitzpatrick_scale: false,
	category: "objects"
};
var gift = {
	keywords: [
		"present",
		"birthday",
		"christmas",
		"xmas"
	],
	char: "🎁",
	fitzpatrick_scale: false,
	category: "objects"
};
var confetti_ball = {
	keywords: [
		"festival",
		"party",
		"birthday",
		"circus"
	],
	char: "🎊",
	fitzpatrick_scale: false,
	category: "objects"
};
var tada = {
	keywords: [
		"party",
		"congratulations",
		"birthday",
		"magic",
		"circus",
		"celebration"
	],
	char: "🎉",
	fitzpatrick_scale: false,
	category: "objects"
};
var dolls = {
	keywords: [
		"japanese",
		"toy",
		"kimono"
	],
	char: "🎎",
	fitzpatrick_scale: false,
	category: "objects"
};
var wind_chime = {
	keywords: [
		"nature",
		"ding",
		"spring",
		"bell"
	],
	char: "🎐",
	fitzpatrick_scale: false,
	category: "objects"
};
var crossed_flags = {
	keywords: [
		"japanese",
		"nation",
		"country",
		"border"
	],
	char: "🎌",
	fitzpatrick_scale: false,
	category: "objects"
};
var izakaya_lantern = {
	keywords: [
		"light",
		"paper",
		"halloween",
		"spooky"
	],
	char: "🏮",
	fitzpatrick_scale: false,
	category: "objects"
};
var red_envelope = {
	keywords: [
		"gift"
	],
	char: "🧧",
	fitzpatrick_scale: false,
	category: "objects"
};
var email = {
	keywords: [
		"letter",
		"postal",
		"inbox",
		"communication"
	],
	char: "✉️",
	fitzpatrick_scale: false,
	category: "objects"
};
var envelope_with_arrow = {
	keywords: [
		"email",
		"communication"
	],
	char: "📩",
	fitzpatrick_scale: false,
	category: "objects"
};
var incoming_envelope = {
	keywords: [
		"email",
		"inbox"
	],
	char: "📨",
	fitzpatrick_scale: false,
	category: "objects"
};
var love_letter = {
	keywords: [
		"email",
		"like",
		"affection",
		"envelope",
		"valentines"
	],
	char: "💌",
	fitzpatrick_scale: false,
	category: "objects"
};
var postbox = {
	keywords: [
		"email",
		"letter",
		"envelope"
	],
	char: "📮",
	fitzpatrick_scale: false,
	category: "objects"
};
var mailbox_closed = {
	keywords: [
		"email",
		"communication",
		"inbox"
	],
	char: "📪",
	fitzpatrick_scale: false,
	category: "objects"
};
var mailbox = {
	keywords: [
		"email",
		"inbox",
		"communication"
	],
	char: "📫",
	fitzpatrick_scale: false,
	category: "objects"
};
var mailbox_with_mail = {
	keywords: [
		"email",
		"inbox",
		"communication"
	],
	char: "📬",
	fitzpatrick_scale: false,
	category: "objects"
};
var mailbox_with_no_mail = {
	keywords: [
		"email",
		"inbox"
	],
	char: "📭",
	fitzpatrick_scale: false,
	category: "objects"
};
var postal_horn = {
	keywords: [
		"instrument",
		"music"
	],
	char: "📯",
	fitzpatrick_scale: false,
	category: "objects"
};
var inbox_tray = {
	keywords: [
		"email",
		"documents"
	],
	char: "📥",
	fitzpatrick_scale: false,
	category: "objects"
};
var outbox_tray = {
	keywords: [
		"inbox",
		"email"
	],
	char: "📤",
	fitzpatrick_scale: false,
	category: "objects"
};
var scroll = {
	keywords: [
		"documents",
		"ancient",
		"history",
		"paper"
	],
	char: "📜",
	fitzpatrick_scale: false,
	category: "objects"
};
var page_with_curl = {
	keywords: [
		"documents",
		"office",
		"paper"
	],
	char: "📃",
	fitzpatrick_scale: false,
	category: "objects"
};
var bookmark_tabs = {
	keywords: [
		"favorite",
		"save",
		"order",
		"tidy"
	],
	char: "📑",
	fitzpatrick_scale: false,
	category: "objects"
};
var receipt = {
	keywords: [
		"accounting",
		"expenses"
	],
	char: "🧾",
	fitzpatrick_scale: false,
	category: "objects"
};
var bar_chart = {
	keywords: [
		"graph",
		"presentation",
		"stats"
	],
	char: "📊",
	fitzpatrick_scale: false,
	category: "objects"
};
var chart_with_upwards_trend = {
	keywords: [
		"graph",
		"presentation",
		"stats",
		"recovery",
		"business",
		"economics",
		"money",
		"sales",
		"good",
		"success"
	],
	char: "📈",
	fitzpatrick_scale: false,
	category: "objects"
};
var chart_with_downwards_trend = {
	keywords: [
		"graph",
		"presentation",
		"stats",
		"recession",
		"business",
		"economics",
		"money",
		"sales",
		"bad",
		"failure"
	],
	char: "📉",
	fitzpatrick_scale: false,
	category: "objects"
};
var page_facing_up = {
	keywords: [
		"documents",
		"office",
		"paper",
		"information"
	],
	char: "📄",
	fitzpatrick_scale: false,
	category: "objects"
};
var date = {
	keywords: [
		"calendar",
		"schedule"
	],
	char: "📅",
	fitzpatrick_scale: false,
	category: "objects"
};
var calendar = {
	keywords: [
		"schedule",
		"date",
		"planning"
	],
	char: "📆",
	fitzpatrick_scale: false,
	category: "objects"
};
var spiral_calendar = {
	keywords: [
		"date",
		"schedule",
		"planning"
	],
	char: "🗓",
	fitzpatrick_scale: false,
	category: "objects"
};
var card_index = {
	keywords: [
		"business",
		"stationery"
	],
	char: "📇",
	fitzpatrick_scale: false,
	category: "objects"
};
var card_file_box = {
	keywords: [
		"business",
		"stationery"
	],
	char: "🗃",
	fitzpatrick_scale: false,
	category: "objects"
};
var ballot_box = {
	keywords: [
		"election",
		"vote"
	],
	char: "🗳",
	fitzpatrick_scale: false,
	category: "objects"
};
var file_cabinet = {
	keywords: [
		"filing",
		"organizing"
	],
	char: "🗄",
	fitzpatrick_scale: false,
	category: "objects"
};
var clipboard = {
	keywords: [
		"stationery",
		"documents"
	],
	char: "📋",
	fitzpatrick_scale: false,
	category: "objects"
};
var spiral_notepad = {
	keywords: [
		"memo",
		"stationery"
	],
	char: "🗒",
	fitzpatrick_scale: false,
	category: "objects"
};
var file_folder = {
	keywords: [
		"documents",
		"business",
		"office"
	],
	char: "📁",
	fitzpatrick_scale: false,
	category: "objects"
};
var open_file_folder = {
	keywords: [
		"documents",
		"load"
	],
	char: "📂",
	fitzpatrick_scale: false,
	category: "objects"
};
var card_index_dividers = {
	keywords: [
		"organizing",
		"business",
		"stationery"
	],
	char: "🗂",
	fitzpatrick_scale: false,
	category: "objects"
};
var newspaper_roll = {
	keywords: [
		"press",
		"headline"
	],
	char: "🗞",
	fitzpatrick_scale: false,
	category: "objects"
};
var newspaper = {
	keywords: [
		"press",
		"headline"
	],
	char: "📰",
	fitzpatrick_scale: false,
	category: "objects"
};
var notebook = {
	keywords: [
		"stationery",
		"record",
		"notes",
		"paper",
		"study"
	],
	char: "📓",
	fitzpatrick_scale: false,
	category: "objects"
};
var closed_book = {
	keywords: [
		"read",
		"library",
		"knowledge",
		"textbook",
		"learn"
	],
	char: "📕",
	fitzpatrick_scale: false,
	category: "objects"
};
var green_book = {
	keywords: [
		"read",
		"library",
		"knowledge",
		"study"
	],
	char: "📗",
	fitzpatrick_scale: false,
	category: "objects"
};
var blue_book = {
	keywords: [
		"read",
		"library",
		"knowledge",
		"learn",
		"study"
	],
	char: "📘",
	fitzpatrick_scale: false,
	category: "objects"
};
var orange_book = {
	keywords: [
		"read",
		"library",
		"knowledge",
		"textbook",
		"study"
	],
	char: "📙",
	fitzpatrick_scale: false,
	category: "objects"
};
var notebook_with_decorative_cover = {
	keywords: [
		"classroom",
		"notes",
		"record",
		"paper",
		"study"
	],
	char: "📔",
	fitzpatrick_scale: false,
	category: "objects"
};
var ledger = {
	keywords: [
		"notes",
		"paper"
	],
	char: "📒",
	fitzpatrick_scale: false,
	category: "objects"
};
var books = {
	keywords: [
		"literature",
		"library",
		"study"
	],
	char: "📚",
	fitzpatrick_scale: false,
	category: "objects"
};
var open_book = {
	keywords: [
		"book",
		"read",
		"library",
		"knowledge",
		"literature",
		"learn",
		"study"
	],
	char: "📖",
	fitzpatrick_scale: false,
	category: "objects"
};
var safety_pin = {
	keywords: [
		"diaper"
	],
	char: "🧷",
	fitzpatrick_scale: false,
	category: "objects"
};
var link = {
	keywords: [
		"rings",
		"url"
	],
	char: "🔗",
	fitzpatrick_scale: false,
	category: "objects"
};
var paperclip = {
	keywords: [
		"documents",
		"stationery"
	],
	char: "📎",
	fitzpatrick_scale: false,
	category: "objects"
};
var paperclips = {
	keywords: [
		"documents",
		"stationery"
	],
	char: "🖇",
	fitzpatrick_scale: false,
	category: "objects"
};
var scissors = {
	keywords: [
		"stationery",
		"cut"
	],
	char: "✂️",
	fitzpatrick_scale: false,
	category: "objects"
};
var triangular_ruler = {
	keywords: [
		"stationery",
		"math",
		"architect",
		"sketch"
	],
	char: "📐",
	fitzpatrick_scale: false,
	category: "objects"
};
var straight_ruler = {
	keywords: [
		"stationery",
		"calculate",
		"length",
		"math",
		"school",
		"drawing",
		"architect",
		"sketch"
	],
	char: "📏",
	fitzpatrick_scale: false,
	category: "objects"
};
var abacus = {
	keywords: [
		"calculation"
	],
	char: "🧮",
	fitzpatrick_scale: false,
	category: "objects"
};
var pushpin = {
	keywords: [
		"stationery",
		"mark",
		"here"
	],
	char: "📌",
	fitzpatrick_scale: false,
	category: "objects"
};
var round_pushpin = {
	keywords: [
		"stationery",
		"location",
		"map",
		"here"
	],
	char: "📍",
	fitzpatrick_scale: false,
	category: "objects"
};
var triangular_flag_on_post = {
	keywords: [
		"mark",
		"milestone",
		"place"
	],
	char: "🚩",
	fitzpatrick_scale: false,
	category: "objects"
};
var white_flag = {
	keywords: [
		"losing",
		"loser",
		"lost",
		"surrender",
		"give up",
		"fail"
	],
	char: "🏳",
	fitzpatrick_scale: false,
	category: "objects"
};
var black_flag = {
	keywords: [
		"pirate"
	],
	char: "🏴",
	fitzpatrick_scale: false,
	category: "objects"
};
var rainbow_flag = {
	keywords: [
		"flag",
		"rainbow",
		"pride",
		"gay",
		"lgbt",
		"glbt",
		"queer",
		"homosexual",
		"lesbian",
		"bisexual",
		"transgender"
	],
	char: "🏳️‍🌈",
	fitzpatrick_scale: false,
	category: "objects"
};
var closed_lock_with_key = {
	keywords: [
		"security",
		"privacy"
	],
	char: "🔐",
	fitzpatrick_scale: false,
	category: "objects"
};
var lock = {
	keywords: [
		"security",
		"password",
		"padlock"
	],
	char: "🔒",
	fitzpatrick_scale: false,
	category: "objects"
};
var unlock = {
	keywords: [
		"privacy",
		"security"
	],
	char: "🔓",
	fitzpatrick_scale: false,
	category: "objects"
};
var lock_with_ink_pen = {
	keywords: [
		"security",
		"secret"
	],
	char: "🔏",
	fitzpatrick_scale: false,
	category: "objects"
};
var pen = {
	keywords: [
		"stationery",
		"writing",
		"write"
	],
	char: "🖊",
	fitzpatrick_scale: false,
	category: "objects"
};
var fountain_pen = {
	keywords: [
		"stationery",
		"writing",
		"write"
	],
	char: "🖋",
	fitzpatrick_scale: false,
	category: "objects"
};
var black_nib = {
	keywords: [
		"pen",
		"stationery",
		"writing",
		"write"
	],
	char: "✒️",
	fitzpatrick_scale: false,
	category: "objects"
};
var memo = {
	keywords: [
		"write",
		"documents",
		"stationery",
		"pencil",
		"paper",
		"writing",
		"legal",
		"exam",
		"quiz",
		"test",
		"study",
		"compose"
	],
	char: "📝",
	fitzpatrick_scale: false,
	category: "objects"
};
var pencil2 = {
	keywords: [
		"stationery",
		"write",
		"paper",
		"writing",
		"school",
		"study"
	],
	char: "✏️",
	fitzpatrick_scale: false,
	category: "objects"
};
var crayon = {
	keywords: [
		"drawing",
		"creativity"
	],
	char: "🖍",
	fitzpatrick_scale: false,
	category: "objects"
};
var paintbrush = {
	keywords: [
		"drawing",
		"creativity",
		"art"
	],
	char: "🖌",
	fitzpatrick_scale: false,
	category: "objects"
};
var mag = {
	keywords: [
		"search",
		"zoom",
		"find",
		"detective"
	],
	char: "🔍",
	fitzpatrick_scale: false,
	category: "objects"
};
var mag_right = {
	keywords: [
		"search",
		"zoom",
		"find",
		"detective"
	],
	char: "🔎",
	fitzpatrick_scale: false,
	category: "objects"
};
var heart = {
	keywords: [
		"love",
		"like",
		"valentines"
	],
	char: "❤️",
	fitzpatrick_scale: false,
	category: "symbols"
};
var orange_heart = {
	keywords: [
		"love",
		"like",
		"affection",
		"valentines"
	],
	char: "🧡",
	fitzpatrick_scale: false,
	category: "symbols"
};
var yellow_heart = {
	keywords: [
		"love",
		"like",
		"affection",
		"valentines"
	],
	char: "💛",
	fitzpatrick_scale: false,
	category: "symbols"
};
var green_heart = {
	keywords: [
		"love",
		"like",
		"affection",
		"valentines"
	],
	char: "💚",
	fitzpatrick_scale: false,
	category: "symbols"
};
var blue_heart = {
	keywords: [
		"love",
		"like",
		"affection",
		"valentines"
	],
	char: "💙",
	fitzpatrick_scale: false,
	category: "symbols"
};
var purple_heart = {
	keywords: [
		"love",
		"like",
		"affection",
		"valentines"
	],
	char: "💜",
	fitzpatrick_scale: false,
	category: "symbols"
};
var black_heart = {
	keywords: [
		"evil"
	],
	char: "🖤",
	fitzpatrick_scale: false,
	category: "symbols"
};
var broken_heart = {
	keywords: [
		"sad",
		"sorry",
		"break",
		"heart",
		"heartbreak"
	],
	char: "💔",
	fitzpatrick_scale: false,
	category: "symbols"
};
var heavy_heart_exclamation = {
	keywords: [
		"decoration",
		"love"
	],
	char: "❣",
	fitzpatrick_scale: false,
	category: "symbols"
};
var two_hearts = {
	keywords: [
		"love",
		"like",
		"affection",
		"valentines",
		"heart"
	],
	char: "💕",
	fitzpatrick_scale: false,
	category: "symbols"
};
var revolving_hearts = {
	keywords: [
		"love",
		"like",
		"affection",
		"valentines"
	],
	char: "💞",
	fitzpatrick_scale: false,
	category: "symbols"
};
var heartbeat = {
	keywords: [
		"love",
		"like",
		"affection",
		"valentines",
		"pink",
		"heart"
	],
	char: "💓",
	fitzpatrick_scale: false,
	category: "symbols"
};
var heartpulse = {
	keywords: [
		"like",
		"love",
		"affection",
		"valentines",
		"pink"
	],
	char: "💗",
	fitzpatrick_scale: false,
	category: "symbols"
};
var sparkling_heart = {
	keywords: [
		"love",
		"like",
		"affection",
		"valentines"
	],
	char: "💖",
	fitzpatrick_scale: false,
	category: "symbols"
};
var cupid = {
	keywords: [
		"love",
		"like",
		"heart",
		"affection",
		"valentines"
	],
	char: "💘",
	fitzpatrick_scale: false,
	category: "symbols"
};
var gift_heart = {
	keywords: [
		"love",
		"valentines"
	],
	char: "💝",
	fitzpatrick_scale: false,
	category: "symbols"
};
var heart_decoration = {
	keywords: [
		"purple-square",
		"love",
		"like"
	],
	char: "💟",
	fitzpatrick_scale: false,
	category: "symbols"
};
var peace_symbol = {
	keywords: [
		"hippie"
	],
	char: "☮",
	fitzpatrick_scale: false,
	category: "symbols"
};
var latin_cross = {
	keywords: [
		"christianity"
	],
	char: "✝",
	fitzpatrick_scale: false,
	category: "symbols"
};
var star_and_crescent = {
	keywords: [
		"islam"
	],
	char: "☪",
	fitzpatrick_scale: false,
	category: "symbols"
};
var om = {
	keywords: [
		"hinduism",
		"buddhism",
		"sikhism",
		"jainism"
	],
	char: "🕉",
	fitzpatrick_scale: false,
	category: "symbols"
};
var wheel_of_dharma = {
	keywords: [
		"hinduism",
		"buddhism",
		"sikhism",
		"jainism"
	],
	char: "☸",
	fitzpatrick_scale: false,
	category: "symbols"
};
var star_of_david = {
	keywords: [
		"judaism"
	],
	char: "✡",
	fitzpatrick_scale: false,
	category: "symbols"
};
var six_pointed_star = {
	keywords: [
		"purple-square",
		"religion",
		"jewish",
		"hexagram"
	],
	char: "🔯",
	fitzpatrick_scale: false,
	category: "symbols"
};
var menorah = {
	keywords: [
		"hanukkah",
		"candles",
		"jewish"
	],
	char: "🕎",
	fitzpatrick_scale: false,
	category: "symbols"
};
var yin_yang = {
	keywords: [
		"balance"
	],
	char: "☯",
	fitzpatrick_scale: false,
	category: "symbols"
};
var orthodox_cross = {
	keywords: [
		"suppedaneum",
		"religion"
	],
	char: "☦",
	fitzpatrick_scale: false,
	category: "symbols"
};
var place_of_worship = {
	keywords: [
		"religion",
		"church",
		"temple",
		"prayer"
	],
	char: "🛐",
	fitzpatrick_scale: false,
	category: "symbols"
};
var ophiuchus = {
	keywords: [
		"sign",
		"purple-square",
		"constellation",
		"astrology"
	],
	char: "⛎",
	fitzpatrick_scale: false,
	category: "symbols"
};
var aries = {
	keywords: [
		"sign",
		"purple-square",
		"zodiac",
		"astrology"
	],
	char: "♈",
	fitzpatrick_scale: false,
	category: "symbols"
};
var taurus = {
	keywords: [
		"purple-square",
		"sign",
		"zodiac",
		"astrology"
	],
	char: "♉",
	fitzpatrick_scale: false,
	category: "symbols"
};
var gemini = {
	keywords: [
		"sign",
		"zodiac",
		"purple-square",
		"astrology"
	],
	char: "♊",
	fitzpatrick_scale: false,
	category: "symbols"
};
var cancer = {
	keywords: [
		"sign",
		"zodiac",
		"purple-square",
		"astrology"
	],
	char: "♋",
	fitzpatrick_scale: false,
	category: "symbols"
};
var leo = {
	keywords: [
		"sign",
		"purple-square",
		"zodiac",
		"astrology"
	],
	char: "♌",
	fitzpatrick_scale: false,
	category: "symbols"
};
var virgo = {
	keywords: [
		"sign",
		"zodiac",
		"purple-square",
		"astrology"
	],
	char: "♍",
	fitzpatrick_scale: false,
	category: "symbols"
};
var libra = {
	keywords: [
		"sign",
		"purple-square",
		"zodiac",
		"astrology"
	],
	char: "♎",
	fitzpatrick_scale: false,
	category: "symbols"
};
var scorpius = {
	keywords: [
		"sign",
		"zodiac",
		"purple-square",
		"astrology",
		"scorpio"
	],
	char: "♏",
	fitzpatrick_scale: false,
	category: "symbols"
};
var sagittarius = {
	keywords: [
		"sign",
		"zodiac",
		"purple-square",
		"astrology"
	],
	char: "♐",
	fitzpatrick_scale: false,
	category: "symbols"
};
var capricorn = {
	keywords: [
		"sign",
		"zodiac",
		"purple-square",
		"astrology"
	],
	char: "♑",
	fitzpatrick_scale: false,
	category: "symbols"
};
var aquarius = {
	keywords: [
		"sign",
		"purple-square",
		"zodiac",
		"astrology"
	],
	char: "♒",
	fitzpatrick_scale: false,
	category: "symbols"
};
var pisces = {
	keywords: [
		"purple-square",
		"sign",
		"zodiac",
		"astrology"
	],
	char: "♓",
	fitzpatrick_scale: false,
	category: "symbols"
};
var id = {
	keywords: [
		"purple-square",
		"words"
	],
	char: "🆔",
	fitzpatrick_scale: false,
	category: "symbols"
};
var atom_symbol = {
	keywords: [
		"science",
		"physics",
		"chemistry"
	],
	char: "⚛",
	fitzpatrick_scale: false,
	category: "symbols"
};
var u7a7a = {
	keywords: [
		"kanji",
		"japanese",
		"chinese",
		"empty",
		"sky",
		"blue-square"
	],
	char: "🈳",
	fitzpatrick_scale: false,
	category: "symbols"
};
var u5272 = {
	keywords: [
		"cut",
		"divide",
		"chinese",
		"kanji",
		"pink-square"
	],
	char: "🈹",
	fitzpatrick_scale: false,
	category: "symbols"
};
var radioactive = {
	keywords: [
		"nuclear",
		"danger"
	],
	char: "☢",
	fitzpatrick_scale: false,
	category: "symbols"
};
var biohazard = {
	keywords: [
		"danger"
	],
	char: "☣",
	fitzpatrick_scale: false,
	category: "symbols"
};
var mobile_phone_off = {
	keywords: [
		"mute",
		"orange-square",
		"silence",
		"quiet"
	],
	char: "📴",
	fitzpatrick_scale: false,
	category: "symbols"
};
var vibration_mode = {
	keywords: [
		"orange-square",
		"phone"
	],
	char: "📳",
	fitzpatrick_scale: false,
	category: "symbols"
};
var u6709 = {
	keywords: [
		"orange-square",
		"chinese",
		"have",
		"kanji"
	],
	char: "🈶",
	fitzpatrick_scale: false,
	category: "symbols"
};
var u7121 = {
	keywords: [
		"nothing",
		"chinese",
		"kanji",
		"japanese",
		"orange-square"
	],
	char: "🈚",
	fitzpatrick_scale: false,
	category: "symbols"
};
var u7533 = {
	keywords: [
		"chinese",
		"japanese",
		"kanji",
		"orange-square"
	],
	char: "🈸",
	fitzpatrick_scale: false,
	category: "symbols"
};
var u55b6 = {
	keywords: [
		"japanese",
		"opening hours",
		"orange-square"
	],
	char: "🈺",
	fitzpatrick_scale: false,
	category: "symbols"
};
var u6708 = {
	keywords: [
		"chinese",
		"month",
		"moon",
		"japanese",
		"orange-square",
		"kanji"
	],
	char: "🈷️",
	fitzpatrick_scale: false,
	category: "symbols"
};
var eight_pointed_black_star = {
	keywords: [
		"orange-square",
		"shape",
		"polygon"
	],
	char: "✴️",
	fitzpatrick_scale: false,
	category: "symbols"
};
var vs = {
	keywords: [
		"words",
		"orange-square"
	],
	char: "🆚",
	fitzpatrick_scale: false,
	category: "symbols"
};
var accept = {
	keywords: [
		"ok",
		"good",
		"chinese",
		"kanji",
		"agree",
		"yes",
		"orange-circle"
	],
	char: "🉑",
	fitzpatrick_scale: false,
	category: "symbols"
};
var white_flower = {
	keywords: [
		"japanese",
		"spring"
	],
	char: "💮",
	fitzpatrick_scale: false,
	category: "symbols"
};
var ideograph_advantage = {
	keywords: [
		"chinese",
		"kanji",
		"obtain",
		"get",
		"circle"
	],
	char: "🉐",
	fitzpatrick_scale: false,
	category: "symbols"
};
var secret = {
	keywords: [
		"privacy",
		"chinese",
		"sshh",
		"kanji",
		"red-circle"
	],
	char: "㊙️",
	fitzpatrick_scale: false,
	category: "symbols"
};
var congratulations = {
	keywords: [
		"chinese",
		"kanji",
		"japanese",
		"red-circle"
	],
	char: "㊗️",
	fitzpatrick_scale: false,
	category: "symbols"
};
var u5408 = {
	keywords: [
		"japanese",
		"chinese",
		"join",
		"kanji",
		"red-square"
	],
	char: "🈴",
	fitzpatrick_scale: false,
	category: "symbols"
};
var u6e80 = {
	keywords: [
		"full",
		"chinese",
		"japanese",
		"red-square",
		"kanji"
	],
	char: "🈵",
	fitzpatrick_scale: false,
	category: "symbols"
};
var u7981 = {
	keywords: [
		"kanji",
		"japanese",
		"chinese",
		"forbidden",
		"limit",
		"restricted",
		"red-square"
	],
	char: "🈲",
	fitzpatrick_scale: false,
	category: "symbols"
};
var a = {
	keywords: [
		"red-square",
		"alphabet",
		"letter"
	],
	char: "🅰️",
	fitzpatrick_scale: false,
	category: "symbols"
};
var b = {
	keywords: [
		"red-square",
		"alphabet",
		"letter"
	],
	char: "🅱️",
	fitzpatrick_scale: false,
	category: "symbols"
};
var ab = {
	keywords: [
		"red-square",
		"alphabet"
	],
	char: "🆎",
	fitzpatrick_scale: false,
	category: "symbols"
};
var cl = {
	keywords: [
		"alphabet",
		"words",
		"red-square"
	],
	char: "🆑",
	fitzpatrick_scale: false,
	category: "symbols"
};
var o2 = {
	keywords: [
		"alphabet",
		"red-square",
		"letter"
	],
	char: "🅾️",
	fitzpatrick_scale: false,
	category: "symbols"
};
var sos = {
	keywords: [
		"help",
		"red-square",
		"words",
		"emergency",
		"911"
	],
	char: "🆘",
	fitzpatrick_scale: false,
	category: "symbols"
};
var no_entry = {
	keywords: [
		"limit",
		"security",
		"privacy",
		"bad",
		"denied",
		"stop",
		"circle"
	],
	char: "⛔",
	fitzpatrick_scale: false,
	category: "symbols"
};
var name_badge = {
	keywords: [
		"fire",
		"forbid"
	],
	char: "📛",
	fitzpatrick_scale: false,
	category: "symbols"
};
var no_entry_sign = {
	keywords: [
		"forbid",
		"stop",
		"limit",
		"denied",
		"disallow",
		"circle"
	],
	char: "🚫",
	fitzpatrick_scale: false,
	category: "symbols"
};
var x = {
	keywords: [
		"no",
		"delete",
		"remove",
		"cancel",
		"red"
	],
	char: "❌",
	fitzpatrick_scale: false,
	category: "symbols"
};
var o = {
	keywords: [
		"circle",
		"round"
	],
	char: "⭕",
	fitzpatrick_scale: false,
	category: "symbols"
};
var stop_sign = {
	keywords: [
		"stop"
	],
	char: "🛑",
	fitzpatrick_scale: false,
	category: "symbols"
};
var anger = {
	keywords: [
		"angry",
		"mad"
	],
	char: "💢",
	fitzpatrick_scale: false,
	category: "symbols"
};
var hotsprings = {
	keywords: [
		"bath",
		"warm",
		"relax"
	],
	char: "♨️",
	fitzpatrick_scale: false,
	category: "symbols"
};
var no_pedestrians = {
	keywords: [
		"rules",
		"crossing",
		"walking",
		"circle"
	],
	char: "🚷",
	fitzpatrick_scale: false,
	category: "symbols"
};
var do_not_litter = {
	keywords: [
		"trash",
		"bin",
		"garbage",
		"circle"
	],
	char: "🚯",
	fitzpatrick_scale: false,
	category: "symbols"
};
var no_bicycles = {
	keywords: [
		"cyclist",
		"prohibited",
		"circle"
	],
	char: "🚳",
	fitzpatrick_scale: false,
	category: "symbols"
};
var underage = {
	keywords: [
		"18",
		"drink",
		"pub",
		"night",
		"minor",
		"circle"
	],
	char: "🔞",
	fitzpatrick_scale: false,
	category: "symbols"
};
var no_mobile_phones = {
	keywords: [
		"iphone",
		"mute",
		"circle"
	],
	char: "📵",
	fitzpatrick_scale: false,
	category: "symbols"
};
var exclamation = {
	keywords: [
		"heavy_exclamation_mark",
		"danger",
		"surprise",
		"punctuation",
		"wow",
		"warning"
	],
	char: "❗",
	fitzpatrick_scale: false,
	category: "symbols"
};
var grey_exclamation = {
	keywords: [
		"surprise",
		"punctuation",
		"gray",
		"wow",
		"warning"
	],
	char: "❕",
	fitzpatrick_scale: false,
	category: "symbols"
};
var question = {
	keywords: [
		"doubt",
		"confused"
	],
	char: "❓",
	fitzpatrick_scale: false,
	category: "symbols"
};
var grey_question = {
	keywords: [
		"doubts",
		"gray",
		"huh",
		"confused"
	],
	char: "❔",
	fitzpatrick_scale: false,
	category: "symbols"
};
var bangbang = {
	keywords: [
		"exclamation",
		"surprise"
	],
	char: "‼️",
	fitzpatrick_scale: false,
	category: "symbols"
};
var interrobang = {
	keywords: [
		"wat",
		"punctuation",
		"surprise"
	],
	char: "⁉️",
	fitzpatrick_scale: false,
	category: "symbols"
};
var low_brightness = {
	keywords: [
		"sun",
		"afternoon",
		"warm",
		"summer"
	],
	char: "🔅",
	fitzpatrick_scale: false,
	category: "symbols"
};
var high_brightness = {
	keywords: [
		"sun",
		"light"
	],
	char: "🔆",
	fitzpatrick_scale: false,
	category: "symbols"
};
var trident = {
	keywords: [
		"weapon",
		"spear"
	],
	char: "🔱",
	fitzpatrick_scale: false,
	category: "symbols"
};
var fleur_de_lis = {
	keywords: [
		"decorative",
		"scout"
	],
	char: "⚜",
	fitzpatrick_scale: false,
	category: "symbols"
};
var part_alternation_mark = {
	keywords: [
		"graph",
		"presentation",
		"stats",
		"business",
		"economics",
		"bad"
	],
	char: "〽️",
	fitzpatrick_scale: false,
	category: "symbols"
};
var warning = {
	keywords: [
		"exclamation",
		"wip",
		"alert",
		"error",
		"problem",
		"issue"
	],
	char: "⚠️",
	fitzpatrick_scale: false,
	category: "symbols"
};
var children_crossing = {
	keywords: [
		"school",
		"warning",
		"danger",
		"sign",
		"driving",
		"yellow-diamond"
	],
	char: "🚸",
	fitzpatrick_scale: false,
	category: "symbols"
};
var beginner = {
	keywords: [
		"badge",
		"shield"
	],
	char: "🔰",
	fitzpatrick_scale: false,
	category: "symbols"
};
var recycle = {
	keywords: [
		"arrow",
		"environment",
		"garbage",
		"trash"
	],
	char: "♻️",
	fitzpatrick_scale: false,
	category: "symbols"
};
var u6307 = {
	keywords: [
		"chinese",
		"point",
		"green-square",
		"kanji"
	],
	char: "🈯",
	fitzpatrick_scale: false,
	category: "symbols"
};
var chart = {
	keywords: [
		"green-square",
		"graph",
		"presentation",
		"stats"
	],
	char: "💹",
	fitzpatrick_scale: false,
	category: "symbols"
};
var sparkle = {
	keywords: [
		"stars",
		"green-square",
		"awesome",
		"good",
		"fireworks"
	],
	char: "❇️",
	fitzpatrick_scale: false,
	category: "symbols"
};
var eight_spoked_asterisk = {
	keywords: [
		"star",
		"sparkle",
		"green-square"
	],
	char: "✳️",
	fitzpatrick_scale: false,
	category: "symbols"
};
var negative_squared_cross_mark = {
	keywords: [
		"x",
		"green-square",
		"no",
		"deny"
	],
	char: "❎",
	fitzpatrick_scale: false,
	category: "symbols"
};
var white_check_mark = {
	keywords: [
		"green-square",
		"ok",
		"agree",
		"vote",
		"election",
		"answer",
		"tick"
	],
	char: "✅",
	fitzpatrick_scale: false,
	category: "symbols"
};
var diamond_shape_with_a_dot_inside = {
	keywords: [
		"jewel",
		"blue",
		"gem",
		"crystal",
		"fancy"
	],
	char: "💠",
	fitzpatrick_scale: false,
	category: "symbols"
};
var cyclone = {
	keywords: [
		"weather",
		"swirl",
		"blue",
		"cloud",
		"vortex",
		"spiral",
		"whirlpool",
		"spin",
		"tornado",
		"hurricane",
		"typhoon"
	],
	char: "🌀",
	fitzpatrick_scale: false,
	category: "symbols"
};
var loop = {
	keywords: [
		"tape",
		"cassette"
	],
	char: "➿",
	fitzpatrick_scale: false,
	category: "symbols"
};
var globe_with_meridians = {
	keywords: [
		"earth",
		"international",
		"world",
		"internet",
		"interweb",
		"i18n"
	],
	char: "🌐",
	fitzpatrick_scale: false,
	category: "symbols"
};
var m = {
	keywords: [
		"alphabet",
		"blue-circle",
		"letter"
	],
	char: "Ⓜ️",
	fitzpatrick_scale: false,
	category: "symbols"
};
var atm = {
	keywords: [
		"money",
		"sales",
		"cash",
		"blue-square",
		"payment",
		"bank"
	],
	char: "🏧",
	fitzpatrick_scale: false,
	category: "symbols"
};
var sa = {
	keywords: [
		"japanese",
		"blue-square",
		"katakana"
	],
	char: "🈂️",
	fitzpatrick_scale: false,
	category: "symbols"
};
var passport_control = {
	keywords: [
		"custom",
		"blue-square"
	],
	char: "🛂",
	fitzpatrick_scale: false,
	category: "symbols"
};
var customs = {
	keywords: [
		"passport",
		"border",
		"blue-square"
	],
	char: "🛃",
	fitzpatrick_scale: false,
	category: "symbols"
};
var baggage_claim = {
	keywords: [
		"blue-square",
		"airport",
		"transport"
	],
	char: "🛄",
	fitzpatrick_scale: false,
	category: "symbols"
};
var left_luggage = {
	keywords: [
		"blue-square",
		"travel"
	],
	char: "🛅",
	fitzpatrick_scale: false,
	category: "symbols"
};
var wheelchair = {
	keywords: [
		"blue-square",
		"disabled",
		"a11y",
		"accessibility"
	],
	char: "♿",
	fitzpatrick_scale: false,
	category: "symbols"
};
var no_smoking = {
	keywords: [
		"cigarette",
		"blue-square",
		"smell",
		"smoke"
	],
	char: "🚭",
	fitzpatrick_scale: false,
	category: "symbols"
};
var wc = {
	keywords: [
		"toilet",
		"restroom",
		"blue-square"
	],
	char: "🚾",
	fitzpatrick_scale: false,
	category: "symbols"
};
var parking = {
	keywords: [
		"cars",
		"blue-square",
		"alphabet",
		"letter"
	],
	char: "🅿️",
	fitzpatrick_scale: false,
	category: "symbols"
};
var potable_water = {
	keywords: [
		"blue-square",
		"liquid",
		"restroom",
		"cleaning",
		"faucet"
	],
	char: "🚰",
	fitzpatrick_scale: false,
	category: "symbols"
};
var mens = {
	keywords: [
		"toilet",
		"restroom",
		"wc",
		"blue-square",
		"gender",
		"male"
	],
	char: "🚹",
	fitzpatrick_scale: false,
	category: "symbols"
};
var womens = {
	keywords: [
		"purple-square",
		"woman",
		"female",
		"toilet",
		"loo",
		"restroom",
		"gender"
	],
	char: "🚺",
	fitzpatrick_scale: false,
	category: "symbols"
};
var baby_symbol = {
	keywords: [
		"orange-square",
		"child"
	],
	char: "🚼",
	fitzpatrick_scale: false,
	category: "symbols"
};
var restroom = {
	keywords: [
		"blue-square",
		"toilet",
		"refresh",
		"wc",
		"gender"
	],
	char: "🚻",
	fitzpatrick_scale: false,
	category: "symbols"
};
var put_litter_in_its_place = {
	keywords: [
		"blue-square",
		"sign",
		"human",
		"info"
	],
	char: "🚮",
	fitzpatrick_scale: false,
	category: "symbols"
};
var cinema = {
	keywords: [
		"blue-square",
		"record",
		"film",
		"movie",
		"curtain",
		"stage",
		"theater"
	],
	char: "🎦",
	fitzpatrick_scale: false,
	category: "symbols"
};
var signal_strength = {
	keywords: [
		"blue-square",
		"reception",
		"phone",
		"internet",
		"connection",
		"wifi",
		"bluetooth",
		"bars"
	],
	char: "📶",
	fitzpatrick_scale: false,
	category: "symbols"
};
var koko = {
	keywords: [
		"blue-square",
		"here",
		"katakana",
		"japanese",
		"destination"
	],
	char: "🈁",
	fitzpatrick_scale: false,
	category: "symbols"
};
var ng = {
	keywords: [
		"blue-square",
		"words",
		"shape",
		"icon"
	],
	char: "🆖",
	fitzpatrick_scale: false,
	category: "symbols"
};
var ok = {
	keywords: [
		"good",
		"agree",
		"yes",
		"blue-square"
	],
	char: "🆗",
	fitzpatrick_scale: false,
	category: "symbols"
};
var up = {
	keywords: [
		"blue-square",
		"above",
		"high"
	],
	char: "🆙",
	fitzpatrick_scale: false,
	category: "symbols"
};
var cool = {
	keywords: [
		"words",
		"blue-square"
	],
	char: "🆒",
	fitzpatrick_scale: false,
	category: "symbols"
};
var free = {
	keywords: [
		"blue-square",
		"words"
	],
	char: "🆓",
	fitzpatrick_scale: false,
	category: "symbols"
};
var zero = {
	keywords: [
		"0",
		"numbers",
		"blue-square",
		"null"
	],
	char: "0️⃣",
	fitzpatrick_scale: false,
	category: "symbols"
};
var one = {
	keywords: [
		"blue-square",
		"numbers",
		"1"
	],
	char: "1️⃣",
	fitzpatrick_scale: false,
	category: "symbols"
};
var two = {
	keywords: [
		"numbers",
		"2",
		"prime",
		"blue-square"
	],
	char: "2️⃣",
	fitzpatrick_scale: false,
	category: "symbols"
};
var three = {
	keywords: [
		"3",
		"numbers",
		"prime",
		"blue-square"
	],
	char: "3️⃣",
	fitzpatrick_scale: false,
	category: "symbols"
};
var four = {
	keywords: [
		"4",
		"numbers",
		"blue-square"
	],
	char: "4️⃣",
	fitzpatrick_scale: false,
	category: "symbols"
};
var five = {
	keywords: [
		"5",
		"numbers",
		"blue-square",
		"prime"
	],
	char: "5️⃣",
	fitzpatrick_scale: false,
	category: "symbols"
};
var six = {
	keywords: [
		"6",
		"numbers",
		"blue-square"
	],
	char: "6️⃣",
	fitzpatrick_scale: false,
	category: "symbols"
};
var seven = {
	keywords: [
		"7",
		"numbers",
		"blue-square",
		"prime"
	],
	char: "7️⃣",
	fitzpatrick_scale: false,
	category: "symbols"
};
var eight = {
	keywords: [
		"8",
		"blue-square",
		"numbers"
	],
	char: "8️⃣",
	fitzpatrick_scale: false,
	category: "symbols"
};
var nine = {
	keywords: [
		"blue-square",
		"numbers",
		"9"
	],
	char: "9️⃣",
	fitzpatrick_scale: false,
	category: "symbols"
};
var keycap_ten = {
	keywords: [
		"numbers",
		"10",
		"blue-square"
	],
	char: "🔟",
	fitzpatrick_scale: false,
	category: "symbols"
};
var asterisk = {
	keywords: [
		"star",
		"keycap"
	],
	char: "*⃣",
	fitzpatrick_scale: false,
	category: "symbols"
};
var eject_button = {
	keywords: [
		"blue-square"
	],
	char: "⏏️",
	fitzpatrick_scale: false,
	category: "symbols"
};
var arrow_forward = {
	keywords: [
		"blue-square",
		"right",
		"direction",
		"play"
	],
	char: "▶️",
	fitzpatrick_scale: false,
	category: "symbols"
};
var pause_button = {
	keywords: [
		"pause",
		"blue-square"
	],
	char: "⏸",
	fitzpatrick_scale: false,
	category: "symbols"
};
var next_track_button = {
	keywords: [
		"forward",
		"next",
		"blue-square"
	],
	char: "⏭",
	fitzpatrick_scale: false,
	category: "symbols"
};
var stop_button = {
	keywords: [
		"blue-square"
	],
	char: "⏹",
	fitzpatrick_scale: false,
	category: "symbols"
};
var record_button = {
	keywords: [
		"blue-square"
	],
	char: "⏺",
	fitzpatrick_scale: false,
	category: "symbols"
};
var play_or_pause_button = {
	keywords: [
		"blue-square",
		"play",
		"pause"
	],
	char: "⏯",
	fitzpatrick_scale: false,
	category: "symbols"
};
var previous_track_button = {
	keywords: [
		"backward"
	],
	char: "⏮",
	fitzpatrick_scale: false,
	category: "symbols"
};
var fast_forward = {
	keywords: [
		"blue-square",
		"play",
		"speed",
		"continue"
	],
	char: "⏩",
	fitzpatrick_scale: false,
	category: "symbols"
};
var rewind = {
	keywords: [
		"play",
		"blue-square"
	],
	char: "⏪",
	fitzpatrick_scale: false,
	category: "symbols"
};
var twisted_rightwards_arrows = {
	keywords: [
		"blue-square",
		"shuffle",
		"music",
		"random"
	],
	char: "🔀",
	fitzpatrick_scale: false,
	category: "symbols"
};
var repeat = {
	keywords: [
		"loop",
		"record"
	],
	char: "🔁",
	fitzpatrick_scale: false,
	category: "symbols"
};
var repeat_one = {
	keywords: [
		"blue-square",
		"loop"
	],
	char: "🔂",
	fitzpatrick_scale: false,
	category: "symbols"
};
var arrow_backward = {
	keywords: [
		"blue-square",
		"left",
		"direction"
	],
	char: "◀️",
	fitzpatrick_scale: false,
	category: "symbols"
};
var arrow_up_small = {
	keywords: [
		"blue-square",
		"triangle",
		"direction",
		"point",
		"forward",
		"top"
	],
	char: "🔼",
	fitzpatrick_scale: false,
	category: "symbols"
};
var arrow_down_small = {
	keywords: [
		"blue-square",
		"direction",
		"bottom"
	],
	char: "🔽",
	fitzpatrick_scale: false,
	category: "symbols"
};
var arrow_double_up = {
	keywords: [
		"blue-square",
		"direction",
		"top"
	],
	char: "⏫",
	fitzpatrick_scale: false,
	category: "symbols"
};
var arrow_double_down = {
	keywords: [
		"blue-square",
		"direction",
		"bottom"
	],
	char: "⏬",
	fitzpatrick_scale: false,
	category: "symbols"
};
var arrow_right = {
	keywords: [
		"blue-square",
		"next"
	],
	char: "➡️",
	fitzpatrick_scale: false,
	category: "symbols"
};
var arrow_left = {
	keywords: [
		"blue-square",
		"previous",
		"back"
	],
	char: "⬅️",
	fitzpatrick_scale: false,
	category: "symbols"
};
var arrow_up = {
	keywords: [
		"blue-square",
		"continue",
		"top",
		"direction"
	],
	char: "⬆️",
	fitzpatrick_scale: false,
	category: "symbols"
};
var arrow_down = {
	keywords: [
		"blue-square",
		"direction",
		"bottom"
	],
	char: "⬇️",
	fitzpatrick_scale: false,
	category: "symbols"
};
var arrow_upper_right = {
	keywords: [
		"blue-square",
		"point",
		"direction",
		"diagonal",
		"northeast"
	],
	char: "↗️",
	fitzpatrick_scale: false,
	category: "symbols"
};
var arrow_lower_right = {
	keywords: [
		"blue-square",
		"direction",
		"diagonal",
		"southeast"
	],
	char: "↘️",
	fitzpatrick_scale: false,
	category: "symbols"
};
var arrow_lower_left = {
	keywords: [
		"blue-square",
		"direction",
		"diagonal",
		"southwest"
	],
	char: "↙️",
	fitzpatrick_scale: false,
	category: "symbols"
};
var arrow_upper_left = {
	keywords: [
		"blue-square",
		"point",
		"direction",
		"diagonal",
		"northwest"
	],
	char: "↖️",
	fitzpatrick_scale: false,
	category: "symbols"
};
var arrow_up_down = {
	keywords: [
		"blue-square",
		"direction",
		"way",
		"vertical"
	],
	char: "↕️",
	fitzpatrick_scale: false,
	category: "symbols"
};
var left_right_arrow = {
	keywords: [
		"shape",
		"direction",
		"horizontal",
		"sideways"
	],
	char: "↔️",
	fitzpatrick_scale: false,
	category: "symbols"
};
var arrows_counterclockwise = {
	keywords: [
		"blue-square",
		"sync",
		"cycle"
	],
	char: "🔄",
	fitzpatrick_scale: false,
	category: "symbols"
};
var arrow_right_hook = {
	keywords: [
		"blue-square",
		"return",
		"rotate",
		"direction"
	],
	char: "↪️",
	fitzpatrick_scale: false,
	category: "symbols"
};
var leftwards_arrow_with_hook = {
	keywords: [
		"back",
		"return",
		"blue-square",
		"undo",
		"enter"
	],
	char: "↩️",
	fitzpatrick_scale: false,
	category: "symbols"
};
var arrow_heading_up = {
	keywords: [
		"blue-square",
		"direction",
		"top"
	],
	char: "⤴️",
	fitzpatrick_scale: false,
	category: "symbols"
};
var arrow_heading_down = {
	keywords: [
		"blue-square",
		"direction",
		"bottom"
	],
	char: "⤵️",
	fitzpatrick_scale: false,
	category: "symbols"
};
var hash = {
	keywords: [
		"symbol",
		"blue-square",
		"twitter"
	],
	char: "#️⃣",
	fitzpatrick_scale: false,
	category: "symbols"
};
var information_source = {
	keywords: [
		"blue-square",
		"alphabet",
		"letter"
	],
	char: "ℹ️",
	fitzpatrick_scale: false,
	category: "symbols"
};
var abc = {
	keywords: [
		"blue-square",
		"alphabet"
	],
	char: "🔤",
	fitzpatrick_scale: false,
	category: "symbols"
};
var abcd = {
	keywords: [
		"blue-square",
		"alphabet"
	],
	char: "🔡",
	fitzpatrick_scale: false,
	category: "symbols"
};
var capital_abcd = {
	keywords: [
		"alphabet",
		"words",
		"blue-square"
	],
	char: "🔠",
	fitzpatrick_scale: false,
	category: "symbols"
};
var symbols = {
	keywords: [
		"blue-square",
		"music",
		"note",
		"ampersand",
		"percent",
		"glyphs",
		"characters"
	],
	char: "🔣",
	fitzpatrick_scale: false,
	category: "symbols"
};
var musical_note = {
	keywords: [
		"score",
		"tone",
		"sound"
	],
	char: "🎵",
	fitzpatrick_scale: false,
	category: "symbols"
};
var notes = {
	keywords: [
		"music",
		"score"
	],
	char: "🎶",
	fitzpatrick_scale: false,
	category: "symbols"
};
var wavy_dash = {
	keywords: [
		"draw",
		"line",
		"moustache",
		"mustache",
		"squiggle",
		"scribble"
	],
	char: "〰️",
	fitzpatrick_scale: false,
	category: "symbols"
};
var curly_loop = {
	keywords: [
		"scribble",
		"draw",
		"shape",
		"squiggle"
	],
	char: "➰",
	fitzpatrick_scale: false,
	category: "symbols"
};
var heavy_check_mark = {
	keywords: [
		"ok",
		"nike",
		"answer",
		"yes",
		"tick"
	],
	char: "✔️",
	fitzpatrick_scale: false,
	category: "symbols"
};
var arrows_clockwise = {
	keywords: [
		"sync",
		"cycle",
		"round",
		"repeat"
	],
	char: "🔃",
	fitzpatrick_scale: false,
	category: "symbols"
};
var heavy_plus_sign = {
	keywords: [
		"math",
		"calculation",
		"addition",
		"more",
		"increase"
	],
	char: "➕",
	fitzpatrick_scale: false,
	category: "symbols"
};
var heavy_minus_sign = {
	keywords: [
		"math",
		"calculation",
		"subtract",
		"less"
	],
	char: "➖",
	fitzpatrick_scale: false,
	category: "symbols"
};
var heavy_division_sign = {
	keywords: [
		"divide",
		"math",
		"calculation"
	],
	char: "➗",
	fitzpatrick_scale: false,
	category: "symbols"
};
var heavy_multiplication_x = {
	keywords: [
		"math",
		"calculation"
	],
	char: "✖️",
	fitzpatrick_scale: false,
	category: "symbols"
};
var infinity = {
	keywords: [
		"forever"
	],
	char: "♾",
	fitzpatrick_scale: false,
	category: "symbols"
};
var heavy_dollar_sign = {
	keywords: [
		"money",
		"sales",
		"payment",
		"currency",
		"buck"
	],
	char: "💲",
	fitzpatrick_scale: false,
	category: "symbols"
};
var currency_exchange = {
	keywords: [
		"money",
		"sales",
		"dollar",
		"travel"
	],
	char: "💱",
	fitzpatrick_scale: false,
	category: "symbols"
};
var copyright = {
	keywords: [
		"ip",
		"license",
		"circle",
		"law",
		"legal"
	],
	char: "©️",
	fitzpatrick_scale: false,
	category: "symbols"
};
var registered = {
	keywords: [
		"alphabet",
		"circle"
	],
	char: "®️",
	fitzpatrick_scale: false,
	category: "symbols"
};
var tm = {
	keywords: [
		"trademark",
		"brand",
		"law",
		"legal"
	],
	char: "™️",
	fitzpatrick_scale: false,
	category: "symbols"
};
var end = {
	keywords: [
		"words",
		"arrow"
	],
	char: "🔚",
	fitzpatrick_scale: false,
	category: "symbols"
};
var back = {
	keywords: [
		"arrow",
		"words",
		"return"
	],
	char: "🔙",
	fitzpatrick_scale: false,
	category: "symbols"
};
var on = {
	keywords: [
		"arrow",
		"words"
	],
	char: "🔛",
	fitzpatrick_scale: false,
	category: "symbols"
};
var top = {
	keywords: [
		"words",
		"blue-square"
	],
	char: "🔝",
	fitzpatrick_scale: false,
	category: "symbols"
};
var soon = {
	keywords: [
		"arrow",
		"words"
	],
	char: "🔜",
	fitzpatrick_scale: false,
	category: "symbols"
};
var ballot_box_with_check = {
	keywords: [
		"ok",
		"agree",
		"confirm",
		"black-square",
		"vote",
		"election",
		"yes",
		"tick"
	],
	char: "☑️",
	fitzpatrick_scale: false,
	category: "symbols"
};
var radio_button = {
	keywords: [
		"input",
		"old",
		"music",
		"circle"
	],
	char: "🔘",
	fitzpatrick_scale: false,
	category: "symbols"
};
var white_circle = {
	keywords: [
		"shape",
		"round"
	],
	char: "⚪",
	fitzpatrick_scale: false,
	category: "symbols"
};
var black_circle = {
	keywords: [
		"shape",
		"button",
		"round"
	],
	char: "⚫",
	fitzpatrick_scale: false,
	category: "symbols"
};
var red_circle = {
	keywords: [
		"shape",
		"error",
		"danger"
	],
	char: "🔴",
	fitzpatrick_scale: false,
	category: "symbols"
};
var large_blue_circle = {
	keywords: [
		"shape",
		"icon",
		"button"
	],
	char: "🔵",
	fitzpatrick_scale: false,
	category: "symbols"
};
var small_orange_diamond = {
	keywords: [
		"shape",
		"jewel",
		"gem"
	],
	char: "🔸",
	fitzpatrick_scale: false,
	category: "symbols"
};
var small_blue_diamond = {
	keywords: [
		"shape",
		"jewel",
		"gem"
	],
	char: "🔹",
	fitzpatrick_scale: false,
	category: "symbols"
};
var large_orange_diamond = {
	keywords: [
		"shape",
		"jewel",
		"gem"
	],
	char: "🔶",
	fitzpatrick_scale: false,
	category: "symbols"
};
var large_blue_diamond = {
	keywords: [
		"shape",
		"jewel",
		"gem"
	],
	char: "🔷",
	fitzpatrick_scale: false,
	category: "symbols"
};
var small_red_triangle = {
	keywords: [
		"shape",
		"direction",
		"up",
		"top"
	],
	char: "🔺",
	fitzpatrick_scale: false,
	category: "symbols"
};
var black_small_square = {
	keywords: [
		"shape",
		"icon"
	],
	char: "▪️",
	fitzpatrick_scale: false,
	category: "symbols"
};
var white_small_square = {
	keywords: [
		"shape",
		"icon"
	],
	char: "▫️",
	fitzpatrick_scale: false,
	category: "symbols"
};
var black_large_square = {
	keywords: [
		"shape",
		"icon",
		"button"
	],
	char: "⬛",
	fitzpatrick_scale: false,
	category: "symbols"
};
var white_large_square = {
	keywords: [
		"shape",
		"icon",
		"stone",
		"button"
	],
	char: "⬜",
	fitzpatrick_scale: false,
	category: "symbols"
};
var small_red_triangle_down = {
	keywords: [
		"shape",
		"direction",
		"bottom"
	],
	char: "🔻",
	fitzpatrick_scale: false,
	category: "symbols"
};
var black_medium_square = {
	keywords: [
		"shape",
		"button",
		"icon"
	],
	char: "◼️",
	fitzpatrick_scale: false,
	category: "symbols"
};
var white_medium_square = {
	keywords: [
		"shape",
		"stone",
		"icon"
	],
	char: "◻️",
	fitzpatrick_scale: false,
	category: "symbols"
};
var black_medium_small_square = {
	keywords: [
		"icon",
		"shape",
		"button"
	],
	char: "◾",
	fitzpatrick_scale: false,
	category: "symbols"
};
var white_medium_small_square = {
	keywords: [
		"shape",
		"stone",
		"icon",
		"button"
	],
	char: "◽",
	fitzpatrick_scale: false,
	category: "symbols"
};
var black_square_button = {
	keywords: [
		"shape",
		"input",
		"frame"
	],
	char: "🔲",
	fitzpatrick_scale: false,
	category: "symbols"
};
var white_square_button = {
	keywords: [
		"shape",
		"input"
	],
	char: "🔳",
	fitzpatrick_scale: false,
	category: "symbols"
};
var speaker = {
	keywords: [
		"sound",
		"volume",
		"silence",
		"broadcast"
	],
	char: "🔈",
	fitzpatrick_scale: false,
	category: "symbols"
};
var sound = {
	keywords: [
		"volume",
		"speaker",
		"broadcast"
	],
	char: "🔉",
	fitzpatrick_scale: false,
	category: "symbols"
};
var loud_sound = {
	keywords: [
		"volume",
		"noise",
		"noisy",
		"speaker",
		"broadcast"
	],
	char: "🔊",
	fitzpatrick_scale: false,
	category: "symbols"
};
var mute = {
	keywords: [
		"sound",
		"volume",
		"silence",
		"quiet"
	],
	char: "🔇",
	fitzpatrick_scale: false,
	category: "symbols"
};
var mega = {
	keywords: [
		"sound",
		"speaker",
		"volume"
	],
	char: "📣",
	fitzpatrick_scale: false,
	category: "symbols"
};
var loudspeaker = {
	keywords: [
		"volume",
		"sound"
	],
	char: "📢",
	fitzpatrick_scale: false,
	category: "symbols"
};
var bell = {
	keywords: [
		"sound",
		"notification",
		"christmas",
		"xmas",
		"chime"
	],
	char: "🔔",
	fitzpatrick_scale: false,
	category: "symbols"
};
var no_bell = {
	keywords: [
		"sound",
		"volume",
		"mute",
		"quiet",
		"silent"
	],
	char: "🔕",
	fitzpatrick_scale: false,
	category: "symbols"
};
var black_joker = {
	keywords: [
		"poker",
		"cards",
		"game",
		"play",
		"magic"
	],
	char: "🃏",
	fitzpatrick_scale: false,
	category: "symbols"
};
var mahjong = {
	keywords: [
		"game",
		"play",
		"chinese",
		"kanji"
	],
	char: "🀄",
	fitzpatrick_scale: false,
	category: "symbols"
};
var spades = {
	keywords: [
		"poker",
		"cards",
		"suits",
		"magic"
	],
	char: "♠️",
	fitzpatrick_scale: false,
	category: "symbols"
};
var clubs = {
	keywords: [
		"poker",
		"cards",
		"magic",
		"suits"
	],
	char: "♣️",
	fitzpatrick_scale: false,
	category: "symbols"
};
var hearts = {
	keywords: [
		"poker",
		"cards",
		"magic",
		"suits"
	],
	char: "♥️",
	fitzpatrick_scale: false,
	category: "symbols"
};
var diamonds = {
	keywords: [
		"poker",
		"cards",
		"magic",
		"suits"
	],
	char: "♦️",
	fitzpatrick_scale: false,
	category: "symbols"
};
var flower_playing_cards = {
	keywords: [
		"game",
		"sunset",
		"red"
	],
	char: "🎴",
	fitzpatrick_scale: false,
	category: "symbols"
};
var thought_balloon = {
	keywords: [
		"bubble",
		"cloud",
		"speech",
		"thinking",
		"dream"
	],
	char: "💭",
	fitzpatrick_scale: false,
	category: "symbols"
};
var right_anger_bubble = {
	keywords: [
		"caption",
		"speech",
		"thinking",
		"mad"
	],
	char: "🗯",
	fitzpatrick_scale: false,
	category: "symbols"
};
var speech_balloon = {
	keywords: [
		"bubble",
		"words",
		"message",
		"talk",
		"chatting"
	],
	char: "💬",
	fitzpatrick_scale: false,
	category: "symbols"
};
var left_speech_bubble = {
	keywords: [
		"words",
		"message",
		"talk",
		"chatting"
	],
	char: "🗨",
	fitzpatrick_scale: false,
	category: "symbols"
};
var clock1 = {
	keywords: [
		"time",
		"late",
		"early",
		"schedule"
	],
	char: "🕐",
	fitzpatrick_scale: false,
	category: "symbols"
};
var clock2 = {
	keywords: [
		"time",
		"late",
		"early",
		"schedule"
	],
	char: "🕑",
	fitzpatrick_scale: false,
	category: "symbols"
};
var clock3 = {
	keywords: [
		"time",
		"late",
		"early",
		"schedule"
	],
	char: "🕒",
	fitzpatrick_scale: false,
	category: "symbols"
};
var clock4 = {
	keywords: [
		"time",
		"late",
		"early",
		"schedule"
	],
	char: "🕓",
	fitzpatrick_scale: false,
	category: "symbols"
};
var clock5 = {
	keywords: [
		"time",
		"late",
		"early",
		"schedule"
	],
	char: "🕔",
	fitzpatrick_scale: false,
	category: "symbols"
};
var clock6 = {
	keywords: [
		"time",
		"late",
		"early",
		"schedule",
		"dawn",
		"dusk"
	],
	char: "🕕",
	fitzpatrick_scale: false,
	category: "symbols"
};
var clock7 = {
	keywords: [
		"time",
		"late",
		"early",
		"schedule"
	],
	char: "🕖",
	fitzpatrick_scale: false,
	category: "symbols"
};
var clock8 = {
	keywords: [
		"time",
		"late",
		"early",
		"schedule"
	],
	char: "🕗",
	fitzpatrick_scale: false,
	category: "symbols"
};
var clock9 = {
	keywords: [
		"time",
		"late",
		"early",
		"schedule"
	],
	char: "🕘",
	fitzpatrick_scale: false,
	category: "symbols"
};
var clock10 = {
	keywords: [
		"time",
		"late",
		"early",
		"schedule"
	],
	char: "🕙",
	fitzpatrick_scale: false,
	category: "symbols"
};
var clock11 = {
	keywords: [
		"time",
		"late",
		"early",
		"schedule"
	],
	char: "🕚",
	fitzpatrick_scale: false,
	category: "symbols"
};
var clock12 = {
	keywords: [
		"time",
		"noon",
		"midnight",
		"midday",
		"late",
		"early",
		"schedule"
	],
	char: "🕛",
	fitzpatrick_scale: false,
	category: "symbols"
};
var clock130 = {
	keywords: [
		"time",
		"late",
		"early",
		"schedule"
	],
	char: "🕜",
	fitzpatrick_scale: false,
	category: "symbols"
};
var clock230 = {
	keywords: [
		"time",
		"late",
		"early",
		"schedule"
	],
	char: "🕝",
	fitzpatrick_scale: false,
	category: "symbols"
};
var clock330 = {
	keywords: [
		"time",
		"late",
		"early",
		"schedule"
	],
	char: "🕞",
	fitzpatrick_scale: false,
	category: "symbols"
};
var clock430 = {
	keywords: [
		"time",
		"late",
		"early",
		"schedule"
	],
	char: "🕟",
	fitzpatrick_scale: false,
	category: "symbols"
};
var clock530 = {
	keywords: [
		"time",
		"late",
		"early",
		"schedule"
	],
	char: "🕠",
	fitzpatrick_scale: false,
	category: "symbols"
};
var clock630 = {
	keywords: [
		"time",
		"late",
		"early",
		"schedule"
	],
	char: "🕡",
	fitzpatrick_scale: false,
	category: "symbols"
};
var clock730 = {
	keywords: [
		"time",
		"late",
		"early",
		"schedule"
	],
	char: "🕢",
	fitzpatrick_scale: false,
	category: "symbols"
};
var clock830 = {
	keywords: [
		"time",
		"late",
		"early",
		"schedule"
	],
	char: "🕣",
	fitzpatrick_scale: false,
	category: "symbols"
};
var clock930 = {
	keywords: [
		"time",
		"late",
		"early",
		"schedule"
	],
	char: "🕤",
	fitzpatrick_scale: false,
	category: "symbols"
};
var clock1030 = {
	keywords: [
		"time",
		"late",
		"early",
		"schedule"
	],
	char: "🕥",
	fitzpatrick_scale: false,
	category: "symbols"
};
var clock1130 = {
	keywords: [
		"time",
		"late",
		"early",
		"schedule"
	],
	char: "🕦",
	fitzpatrick_scale: false,
	category: "symbols"
};
var clock1230 = {
	keywords: [
		"time",
		"late",
		"early",
		"schedule"
	],
	char: "🕧",
	fitzpatrick_scale: false,
	category: "symbols"
};
var afghanistan = {
	keywords: [
		"af",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇦🇫",
	fitzpatrick_scale: false,
	category: "flags"
};
var aland_islands = {
	keywords: [
		"Åland",
		"islands",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇦🇽",
	fitzpatrick_scale: false,
	category: "flags"
};
var albania = {
	keywords: [
		"al",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇦🇱",
	fitzpatrick_scale: false,
	category: "flags"
};
var algeria = {
	keywords: [
		"dz",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇩🇿",
	fitzpatrick_scale: false,
	category: "flags"
};
var american_samoa = {
	keywords: [
		"american",
		"ws",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇦🇸",
	fitzpatrick_scale: false,
	category: "flags"
};
var andorra = {
	keywords: [
		"ad",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇦🇩",
	fitzpatrick_scale: false,
	category: "flags"
};
var angola = {
	keywords: [
		"ao",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇦🇴",
	fitzpatrick_scale: false,
	category: "flags"
};
var anguilla = {
	keywords: [
		"ai",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇦🇮",
	fitzpatrick_scale: false,
	category: "flags"
};
var antarctica = {
	keywords: [
		"aq",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇦🇶",
	fitzpatrick_scale: false,
	category: "flags"
};
var antigua_barbuda = {
	keywords: [
		"antigua",
		"barbuda",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇦🇬",
	fitzpatrick_scale: false,
	category: "flags"
};
var argentina = {
	keywords: [
		"ar",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇦🇷",
	fitzpatrick_scale: false,
	category: "flags"
};
var armenia = {
	keywords: [
		"am",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇦🇲",
	fitzpatrick_scale: false,
	category: "flags"
};
var aruba = {
	keywords: [
		"aw",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇦🇼",
	fitzpatrick_scale: false,
	category: "flags"
};
var australia = {
	keywords: [
		"au",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇦🇺",
	fitzpatrick_scale: false,
	category: "flags"
};
var austria = {
	keywords: [
		"at",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇦🇹",
	fitzpatrick_scale: false,
	category: "flags"
};
var azerbaijan = {
	keywords: [
		"az",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇦🇿",
	fitzpatrick_scale: false,
	category: "flags"
};
var bahamas = {
	keywords: [
		"bs",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇧🇸",
	fitzpatrick_scale: false,
	category: "flags"
};
var bahrain = {
	keywords: [
		"bh",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇧🇭",
	fitzpatrick_scale: false,
	category: "flags"
};
var bangladesh = {
	keywords: [
		"bd",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇧🇩",
	fitzpatrick_scale: false,
	category: "flags"
};
var barbados = {
	keywords: [
		"bb",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇧🇧",
	fitzpatrick_scale: false,
	category: "flags"
};
var belarus = {
	keywords: [
		"by",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇧🇾",
	fitzpatrick_scale: false,
	category: "flags"
};
var belgium = {
	keywords: [
		"be",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇧🇪",
	fitzpatrick_scale: false,
	category: "flags"
};
var belize = {
	keywords: [
		"bz",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇧🇿",
	fitzpatrick_scale: false,
	category: "flags"
};
var benin = {
	keywords: [
		"bj",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇧🇯",
	fitzpatrick_scale: false,
	category: "flags"
};
var bermuda = {
	keywords: [
		"bm",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇧🇲",
	fitzpatrick_scale: false,
	category: "flags"
};
var bhutan = {
	keywords: [
		"bt",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇧🇹",
	fitzpatrick_scale: false,
	category: "flags"
};
var bolivia = {
	keywords: [
		"bo",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇧🇴",
	fitzpatrick_scale: false,
	category: "flags"
};
var caribbean_netherlands = {
	keywords: [
		"bonaire",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇧🇶",
	fitzpatrick_scale: false,
	category: "flags"
};
var bosnia_herzegovina = {
	keywords: [
		"bosnia",
		"herzegovina",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇧🇦",
	fitzpatrick_scale: false,
	category: "flags"
};
var botswana = {
	keywords: [
		"bw",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇧🇼",
	fitzpatrick_scale: false,
	category: "flags"
};
var brazil = {
	keywords: [
		"br",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇧🇷",
	fitzpatrick_scale: false,
	category: "flags"
};
var british_indian_ocean_territory = {
	keywords: [
		"british",
		"indian",
		"ocean",
		"territory",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇮🇴",
	fitzpatrick_scale: false,
	category: "flags"
};
var british_virgin_islands = {
	keywords: [
		"british",
		"virgin",
		"islands",
		"bvi",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇻🇬",
	fitzpatrick_scale: false,
	category: "flags"
};
var brunei = {
	keywords: [
		"bn",
		"darussalam",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇧🇳",
	fitzpatrick_scale: false,
	category: "flags"
};
var bulgaria = {
	keywords: [
		"bg",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇧🇬",
	fitzpatrick_scale: false,
	category: "flags"
};
var burkina_faso = {
	keywords: [
		"burkina",
		"faso",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇧🇫",
	fitzpatrick_scale: false,
	category: "flags"
};
var burundi = {
	keywords: [
		"bi",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇧🇮",
	fitzpatrick_scale: false,
	category: "flags"
};
var cape_verde = {
	keywords: [
		"cabo",
		"verde",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇨🇻",
	fitzpatrick_scale: false,
	category: "flags"
};
var cambodia = {
	keywords: [
		"kh",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇰🇭",
	fitzpatrick_scale: false,
	category: "flags"
};
var cameroon = {
	keywords: [
		"cm",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇨🇲",
	fitzpatrick_scale: false,
	category: "flags"
};
var canada = {
	keywords: [
		"ca",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇨🇦",
	fitzpatrick_scale: false,
	category: "flags"
};
var canary_islands = {
	keywords: [
		"canary",
		"islands",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇮🇨",
	fitzpatrick_scale: false,
	category: "flags"
};
var cayman_islands = {
	keywords: [
		"cayman",
		"islands",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇰🇾",
	fitzpatrick_scale: false,
	category: "flags"
};
var central_african_republic = {
	keywords: [
		"central",
		"african",
		"republic",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇨🇫",
	fitzpatrick_scale: false,
	category: "flags"
};
var chad = {
	keywords: [
		"td",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇹🇩",
	fitzpatrick_scale: false,
	category: "flags"
};
var chile = {
	keywords: [
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇨🇱",
	fitzpatrick_scale: false,
	category: "flags"
};
var cn = {
	keywords: [
		"china",
		"chinese",
		"prc",
		"flag",
		"country",
		"nation",
		"banner"
	],
	char: "🇨🇳",
	fitzpatrick_scale: false,
	category: "flags"
};
var christmas_island = {
	keywords: [
		"christmas",
		"island",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇨🇽",
	fitzpatrick_scale: false,
	category: "flags"
};
var cocos_islands = {
	keywords: [
		"cocos",
		"keeling",
		"islands",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇨🇨",
	fitzpatrick_scale: false,
	category: "flags"
};
var colombia = {
	keywords: [
		"co",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇨🇴",
	fitzpatrick_scale: false,
	category: "flags"
};
var comoros = {
	keywords: [
		"km",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇰🇲",
	fitzpatrick_scale: false,
	category: "flags"
};
var congo_brazzaville = {
	keywords: [
		"congo",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇨🇬",
	fitzpatrick_scale: false,
	category: "flags"
};
var congo_kinshasa = {
	keywords: [
		"congo",
		"democratic",
		"republic",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇨🇩",
	fitzpatrick_scale: false,
	category: "flags"
};
var cook_islands = {
	keywords: [
		"cook",
		"islands",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇨🇰",
	fitzpatrick_scale: false,
	category: "flags"
};
var costa_rica = {
	keywords: [
		"costa",
		"rica",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇨🇷",
	fitzpatrick_scale: false,
	category: "flags"
};
var croatia = {
	keywords: [
		"hr",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇭🇷",
	fitzpatrick_scale: false,
	category: "flags"
};
var cuba = {
	keywords: [
		"cu",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇨🇺",
	fitzpatrick_scale: false,
	category: "flags"
};
var curacao = {
	keywords: [
		"curaçao",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇨🇼",
	fitzpatrick_scale: false,
	category: "flags"
};
var cyprus = {
	keywords: [
		"cy",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇨🇾",
	fitzpatrick_scale: false,
	category: "flags"
};
var czech_republic = {
	keywords: [
		"cz",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇨🇿",
	fitzpatrick_scale: false,
	category: "flags"
};
var denmark = {
	keywords: [
		"dk",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇩🇰",
	fitzpatrick_scale: false,
	category: "flags"
};
var djibouti = {
	keywords: [
		"dj",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇩🇯",
	fitzpatrick_scale: false,
	category: "flags"
};
var dominica = {
	keywords: [
		"dm",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇩🇲",
	fitzpatrick_scale: false,
	category: "flags"
};
var dominican_republic = {
	keywords: [
		"dominican",
		"republic",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇩🇴",
	fitzpatrick_scale: false,
	category: "flags"
};
var ecuador = {
	keywords: [
		"ec",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇪🇨",
	fitzpatrick_scale: false,
	category: "flags"
};
var egypt = {
	keywords: [
		"eg",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇪🇬",
	fitzpatrick_scale: false,
	category: "flags"
};
var el_salvador = {
	keywords: [
		"el",
		"salvador",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇸🇻",
	fitzpatrick_scale: false,
	category: "flags"
};
var equatorial_guinea = {
	keywords: [
		"equatorial",
		"gn",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇬🇶",
	fitzpatrick_scale: false,
	category: "flags"
};
var eritrea = {
	keywords: [
		"er",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇪🇷",
	fitzpatrick_scale: false,
	category: "flags"
};
var estonia = {
	keywords: [
		"ee",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇪🇪",
	fitzpatrick_scale: false,
	category: "flags"
};
var ethiopia = {
	keywords: [
		"et",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇪🇹",
	fitzpatrick_scale: false,
	category: "flags"
};
var eu = {
	keywords: [
		"european",
		"union",
		"flag",
		"banner"
	],
	char: "🇪🇺",
	fitzpatrick_scale: false,
	category: "flags"
};
var falkland_islands = {
	keywords: [
		"falkland",
		"islands",
		"malvinas",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇫🇰",
	fitzpatrick_scale: false,
	category: "flags"
};
var faroe_islands = {
	keywords: [
		"faroe",
		"islands",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇫🇴",
	fitzpatrick_scale: false,
	category: "flags"
};
var fiji = {
	keywords: [
		"fj",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇫🇯",
	fitzpatrick_scale: false,
	category: "flags"
};
var finland = {
	keywords: [
		"fi",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇫🇮",
	fitzpatrick_scale: false,
	category: "flags"
};
var fr = {
	keywords: [
		"banner",
		"flag",
		"nation",
		"france",
		"french",
		"country"
	],
	char: "🇫🇷",
	fitzpatrick_scale: false,
	category: "flags"
};
var french_guiana = {
	keywords: [
		"french",
		"guiana",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇬🇫",
	fitzpatrick_scale: false,
	category: "flags"
};
var french_polynesia = {
	keywords: [
		"french",
		"polynesia",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇵🇫",
	fitzpatrick_scale: false,
	category: "flags"
};
var french_southern_territories = {
	keywords: [
		"french",
		"southern",
		"territories",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇹🇫",
	fitzpatrick_scale: false,
	category: "flags"
};
var gabon = {
	keywords: [
		"ga",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇬🇦",
	fitzpatrick_scale: false,
	category: "flags"
};
var gambia = {
	keywords: [
		"gm",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇬🇲",
	fitzpatrick_scale: false,
	category: "flags"
};
var georgia = {
	keywords: [
		"ge",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇬🇪",
	fitzpatrick_scale: false,
	category: "flags"
};
var de = {
	keywords: [
		"german",
		"nation",
		"flag",
		"country",
		"banner"
	],
	char: "🇩🇪",
	fitzpatrick_scale: false,
	category: "flags"
};
var ghana = {
	keywords: [
		"gh",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇬🇭",
	fitzpatrick_scale: false,
	category: "flags"
};
var gibraltar = {
	keywords: [
		"gi",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇬🇮",
	fitzpatrick_scale: false,
	category: "flags"
};
var greece = {
	keywords: [
		"gr",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇬🇷",
	fitzpatrick_scale: false,
	category: "flags"
};
var greenland = {
	keywords: [
		"gl",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇬🇱",
	fitzpatrick_scale: false,
	category: "flags"
};
var grenada = {
	keywords: [
		"gd",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇬🇩",
	fitzpatrick_scale: false,
	category: "flags"
};
var guadeloupe = {
	keywords: [
		"gp",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇬🇵",
	fitzpatrick_scale: false,
	category: "flags"
};
var guam = {
	keywords: [
		"gu",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇬🇺",
	fitzpatrick_scale: false,
	category: "flags"
};
var guatemala = {
	keywords: [
		"gt",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇬🇹",
	fitzpatrick_scale: false,
	category: "flags"
};
var guernsey = {
	keywords: [
		"gg",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇬🇬",
	fitzpatrick_scale: false,
	category: "flags"
};
var guinea = {
	keywords: [
		"gn",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇬🇳",
	fitzpatrick_scale: false,
	category: "flags"
};
var guinea_bissau = {
	keywords: [
		"gw",
		"bissau",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇬🇼",
	fitzpatrick_scale: false,
	category: "flags"
};
var guyana = {
	keywords: [
		"gy",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇬🇾",
	fitzpatrick_scale: false,
	category: "flags"
};
var haiti = {
	keywords: [
		"ht",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇭🇹",
	fitzpatrick_scale: false,
	category: "flags"
};
var honduras = {
	keywords: [
		"hn",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇭🇳",
	fitzpatrick_scale: false,
	category: "flags"
};
var hong_kong = {
	keywords: [
		"hong",
		"kong",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇭🇰",
	fitzpatrick_scale: false,
	category: "flags"
};
var hungary = {
	keywords: [
		"hu",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇭🇺",
	fitzpatrick_scale: false,
	category: "flags"
};
var iceland = {
	keywords: [
		"is",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇮🇸",
	fitzpatrick_scale: false,
	category: "flags"
};
var india = {
	keywords: [
		"in",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇮🇳",
	fitzpatrick_scale: false,
	category: "flags"
};
var indonesia = {
	keywords: [
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇮🇩",
	fitzpatrick_scale: false,
	category: "flags"
};
var iran = {
	keywords: [
		"iran,",
		"islamic",
		"republic",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇮🇷",
	fitzpatrick_scale: false,
	category: "flags"
};
var iraq = {
	keywords: [
		"iq",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇮🇶",
	fitzpatrick_scale: false,
	category: "flags"
};
var ireland = {
	keywords: [
		"ie",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇮🇪",
	fitzpatrick_scale: false,
	category: "flags"
};
var isle_of_man = {
	keywords: [
		"isle",
		"man",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇮🇲",
	fitzpatrick_scale: false,
	category: "flags"
};
var israel = {
	keywords: [
		"il",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇮🇱",
	fitzpatrick_scale: false,
	category: "flags"
};
var it = {
	keywords: [
		"italy",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇮🇹",
	fitzpatrick_scale: false,
	category: "flags"
};
var cote_divoire = {
	keywords: [
		"ivory",
		"coast",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇨🇮",
	fitzpatrick_scale: false,
	category: "flags"
};
var jamaica = {
	keywords: [
		"jm",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇯🇲",
	fitzpatrick_scale: false,
	category: "flags"
};
var jp = {
	keywords: [
		"japanese",
		"nation",
		"flag",
		"country",
		"banner"
	],
	char: "🇯🇵",
	fitzpatrick_scale: false,
	category: "flags"
};
var jersey = {
	keywords: [
		"je",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇯🇪",
	fitzpatrick_scale: false,
	category: "flags"
};
var jordan = {
	keywords: [
		"jo",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇯🇴",
	fitzpatrick_scale: false,
	category: "flags"
};
var kazakhstan = {
	keywords: [
		"kz",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇰🇿",
	fitzpatrick_scale: false,
	category: "flags"
};
var kenya = {
	keywords: [
		"ke",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇰🇪",
	fitzpatrick_scale: false,
	category: "flags"
};
var kiribati = {
	keywords: [
		"ki",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇰🇮",
	fitzpatrick_scale: false,
	category: "flags"
};
var kosovo = {
	keywords: [
		"xk",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇽🇰",
	fitzpatrick_scale: false,
	category: "flags"
};
var kuwait = {
	keywords: [
		"kw",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇰🇼",
	fitzpatrick_scale: false,
	category: "flags"
};
var kyrgyzstan = {
	keywords: [
		"kg",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇰🇬",
	fitzpatrick_scale: false,
	category: "flags"
};
var laos = {
	keywords: [
		"lao",
		"democratic",
		"republic",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇱🇦",
	fitzpatrick_scale: false,
	category: "flags"
};
var latvia = {
	keywords: [
		"lv",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇱🇻",
	fitzpatrick_scale: false,
	category: "flags"
};
var lebanon = {
	keywords: [
		"lb",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇱🇧",
	fitzpatrick_scale: false,
	category: "flags"
};
var lesotho = {
	keywords: [
		"ls",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇱🇸",
	fitzpatrick_scale: false,
	category: "flags"
};
var liberia = {
	keywords: [
		"lr",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇱🇷",
	fitzpatrick_scale: false,
	category: "flags"
};
var libya = {
	keywords: [
		"ly",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇱🇾",
	fitzpatrick_scale: false,
	category: "flags"
};
var liechtenstein = {
	keywords: [
		"li",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇱🇮",
	fitzpatrick_scale: false,
	category: "flags"
};
var lithuania = {
	keywords: [
		"lt",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇱🇹",
	fitzpatrick_scale: false,
	category: "flags"
};
var luxembourg = {
	keywords: [
		"lu",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇱🇺",
	fitzpatrick_scale: false,
	category: "flags"
};
var macau = {
	keywords: [
		"macao",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇲🇴",
	fitzpatrick_scale: false,
	category: "flags"
};
var macedonia = {
	keywords: [
		"macedonia,",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇲🇰",
	fitzpatrick_scale: false,
	category: "flags"
};
var madagascar = {
	keywords: [
		"mg",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇲🇬",
	fitzpatrick_scale: false,
	category: "flags"
};
var malawi = {
	keywords: [
		"mw",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇲🇼",
	fitzpatrick_scale: false,
	category: "flags"
};
var malaysia = {
	keywords: [
		"my",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇲🇾",
	fitzpatrick_scale: false,
	category: "flags"
};
var maldives = {
	keywords: [
		"mv",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇲🇻",
	fitzpatrick_scale: false,
	category: "flags"
};
var mali = {
	keywords: [
		"ml",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇲🇱",
	fitzpatrick_scale: false,
	category: "flags"
};
var malta = {
	keywords: [
		"mt",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇲🇹",
	fitzpatrick_scale: false,
	category: "flags"
};
var marshall_islands = {
	keywords: [
		"marshall",
		"islands",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇲🇭",
	fitzpatrick_scale: false,
	category: "flags"
};
var martinique = {
	keywords: [
		"mq",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇲🇶",
	fitzpatrick_scale: false,
	category: "flags"
};
var mauritania = {
	keywords: [
		"mr",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇲🇷",
	fitzpatrick_scale: false,
	category: "flags"
};
var mauritius = {
	keywords: [
		"mu",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇲🇺",
	fitzpatrick_scale: false,
	category: "flags"
};
var mayotte = {
	keywords: [
		"yt",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇾🇹",
	fitzpatrick_scale: false,
	category: "flags"
};
var mexico = {
	keywords: [
		"mx",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇲🇽",
	fitzpatrick_scale: false,
	category: "flags"
};
var micronesia = {
	keywords: [
		"micronesia,",
		"federated",
		"states",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇫🇲",
	fitzpatrick_scale: false,
	category: "flags"
};
var moldova = {
	keywords: [
		"moldova,",
		"republic",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇲🇩",
	fitzpatrick_scale: false,
	category: "flags"
};
var monaco = {
	keywords: [
		"mc",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇲🇨",
	fitzpatrick_scale: false,
	category: "flags"
};
var mongolia = {
	keywords: [
		"mn",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇲🇳",
	fitzpatrick_scale: false,
	category: "flags"
};
var montenegro = {
	keywords: [
		"me",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇲🇪",
	fitzpatrick_scale: false,
	category: "flags"
};
var montserrat = {
	keywords: [
		"ms",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇲🇸",
	fitzpatrick_scale: false,
	category: "flags"
};
var morocco = {
	keywords: [
		"ma",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇲🇦",
	fitzpatrick_scale: false,
	category: "flags"
};
var mozambique = {
	keywords: [
		"mz",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇲🇿",
	fitzpatrick_scale: false,
	category: "flags"
};
var myanmar = {
	keywords: [
		"mm",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇲🇲",
	fitzpatrick_scale: false,
	category: "flags"
};
var namibia = {
	keywords: [
		"na",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇳🇦",
	fitzpatrick_scale: false,
	category: "flags"
};
var nauru = {
	keywords: [
		"nr",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇳🇷",
	fitzpatrick_scale: false,
	category: "flags"
};
var nepal = {
	keywords: [
		"np",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇳🇵",
	fitzpatrick_scale: false,
	category: "flags"
};
var netherlands = {
	keywords: [
		"nl",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇳🇱",
	fitzpatrick_scale: false,
	category: "flags"
};
var new_caledonia = {
	keywords: [
		"new",
		"caledonia",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇳🇨",
	fitzpatrick_scale: false,
	category: "flags"
};
var new_zealand = {
	keywords: [
		"new",
		"zealand",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇳🇿",
	fitzpatrick_scale: false,
	category: "flags"
};
var nicaragua = {
	keywords: [
		"ni",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇳🇮",
	fitzpatrick_scale: false,
	category: "flags"
};
var niger = {
	keywords: [
		"ne",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇳🇪",
	fitzpatrick_scale: false,
	category: "flags"
};
var nigeria = {
	keywords: [
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇳🇬",
	fitzpatrick_scale: false,
	category: "flags"
};
var niue = {
	keywords: [
		"nu",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇳🇺",
	fitzpatrick_scale: false,
	category: "flags"
};
var norfolk_island = {
	keywords: [
		"norfolk",
		"island",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇳🇫",
	fitzpatrick_scale: false,
	category: "flags"
};
var northern_mariana_islands = {
	keywords: [
		"northern",
		"mariana",
		"islands",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇲🇵",
	fitzpatrick_scale: false,
	category: "flags"
};
var north_korea = {
	keywords: [
		"north",
		"korea",
		"nation",
		"flag",
		"country",
		"banner"
	],
	char: "🇰🇵",
	fitzpatrick_scale: false,
	category: "flags"
};
var norway = {
	keywords: [
		"no",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇳🇴",
	fitzpatrick_scale: false,
	category: "flags"
};
var oman = {
	keywords: [
		"om_symbol",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇴🇲",
	fitzpatrick_scale: false,
	category: "flags"
};
var pakistan = {
	keywords: [
		"pk",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇵🇰",
	fitzpatrick_scale: false,
	category: "flags"
};
var palau = {
	keywords: [
		"pw",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇵🇼",
	fitzpatrick_scale: false,
	category: "flags"
};
var palestinian_territories = {
	keywords: [
		"palestine",
		"palestinian",
		"territories",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇵🇸",
	fitzpatrick_scale: false,
	category: "flags"
};
var panama = {
	keywords: [
		"pa",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇵🇦",
	fitzpatrick_scale: false,
	category: "flags"
};
var papua_new_guinea = {
	keywords: [
		"papua",
		"new",
		"guinea",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇵🇬",
	fitzpatrick_scale: false,
	category: "flags"
};
var paraguay = {
	keywords: [
		"py",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇵🇾",
	fitzpatrick_scale: false,
	category: "flags"
};
var peru = {
	keywords: [
		"pe",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇵🇪",
	fitzpatrick_scale: false,
	category: "flags"
};
var philippines = {
	keywords: [
		"ph",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇵🇭",
	fitzpatrick_scale: false,
	category: "flags"
};
var pitcairn_islands = {
	keywords: [
		"pitcairn",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇵🇳",
	fitzpatrick_scale: false,
	category: "flags"
};
var poland = {
	keywords: [
		"pl",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇵🇱",
	fitzpatrick_scale: false,
	category: "flags"
};
var portugal = {
	keywords: [
		"pt",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇵🇹",
	fitzpatrick_scale: false,
	category: "flags"
};
var puerto_rico = {
	keywords: [
		"puerto",
		"rico",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇵🇷",
	fitzpatrick_scale: false,
	category: "flags"
};
var qatar = {
	keywords: [
		"qa",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇶🇦",
	fitzpatrick_scale: false,
	category: "flags"
};
var reunion = {
	keywords: [
		"réunion",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇷🇪",
	fitzpatrick_scale: false,
	category: "flags"
};
var romania = {
	keywords: [
		"ro",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇷🇴",
	fitzpatrick_scale: false,
	category: "flags"
};
var ru = {
	keywords: [
		"russian",
		"federation",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇷🇺",
	fitzpatrick_scale: false,
	category: "flags"
};
var rwanda = {
	keywords: [
		"rw",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇷🇼",
	fitzpatrick_scale: false,
	category: "flags"
};
var st_barthelemy = {
	keywords: [
		"saint",
		"barthélemy",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇧🇱",
	fitzpatrick_scale: false,
	category: "flags"
};
var st_helena = {
	keywords: [
		"saint",
		"helena",
		"ascension",
		"tristan",
		"cunha",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇸🇭",
	fitzpatrick_scale: false,
	category: "flags"
};
var st_kitts_nevis = {
	keywords: [
		"saint",
		"kitts",
		"nevis",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇰🇳",
	fitzpatrick_scale: false,
	category: "flags"
};
var st_lucia = {
	keywords: [
		"saint",
		"lucia",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇱🇨",
	fitzpatrick_scale: false,
	category: "flags"
};
var st_pierre_miquelon = {
	keywords: [
		"saint",
		"pierre",
		"miquelon",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇵🇲",
	fitzpatrick_scale: false,
	category: "flags"
};
var st_vincent_grenadines = {
	keywords: [
		"saint",
		"vincent",
		"grenadines",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇻🇨",
	fitzpatrick_scale: false,
	category: "flags"
};
var samoa = {
	keywords: [
		"ws",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇼🇸",
	fitzpatrick_scale: false,
	category: "flags"
};
var san_marino = {
	keywords: [
		"san",
		"marino",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇸🇲",
	fitzpatrick_scale: false,
	category: "flags"
};
var sao_tome_principe = {
	keywords: [
		"sao",
		"tome",
		"principe",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇸🇹",
	fitzpatrick_scale: false,
	category: "flags"
};
var saudi_arabia = {
	keywords: [
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇸🇦",
	fitzpatrick_scale: false,
	category: "flags"
};
var senegal = {
	keywords: [
		"sn",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇸🇳",
	fitzpatrick_scale: false,
	category: "flags"
};
var serbia = {
	keywords: [
		"rs",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇷🇸",
	fitzpatrick_scale: false,
	category: "flags"
};
var seychelles = {
	keywords: [
		"sc",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇸🇨",
	fitzpatrick_scale: false,
	category: "flags"
};
var sierra_leone = {
	keywords: [
		"sierra",
		"leone",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇸🇱",
	fitzpatrick_scale: false,
	category: "flags"
};
var singapore = {
	keywords: [
		"sg",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇸🇬",
	fitzpatrick_scale: false,
	category: "flags"
};
var sint_maarten = {
	keywords: [
		"sint",
		"maarten",
		"dutch",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇸🇽",
	fitzpatrick_scale: false,
	category: "flags"
};
var slovakia = {
	keywords: [
		"sk",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇸🇰",
	fitzpatrick_scale: false,
	category: "flags"
};
var slovenia = {
	keywords: [
		"si",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇸🇮",
	fitzpatrick_scale: false,
	category: "flags"
};
var solomon_islands = {
	keywords: [
		"solomon",
		"islands",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇸🇧",
	fitzpatrick_scale: false,
	category: "flags"
};
var somalia = {
	keywords: [
		"so",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇸🇴",
	fitzpatrick_scale: false,
	category: "flags"
};
var south_africa = {
	keywords: [
		"south",
		"africa",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇿🇦",
	fitzpatrick_scale: false,
	category: "flags"
};
var south_georgia_south_sandwich_islands = {
	keywords: [
		"south",
		"georgia",
		"sandwich",
		"islands",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇬🇸",
	fitzpatrick_scale: false,
	category: "flags"
};
var kr = {
	keywords: [
		"south",
		"korea",
		"nation",
		"flag",
		"country",
		"banner"
	],
	char: "🇰🇷",
	fitzpatrick_scale: false,
	category: "flags"
};
var south_sudan = {
	keywords: [
		"south",
		"sd",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇸🇸",
	fitzpatrick_scale: false,
	category: "flags"
};
var es = {
	keywords: [
		"spain",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇪🇸",
	fitzpatrick_scale: false,
	category: "flags"
};
var sri_lanka = {
	keywords: [
		"sri",
		"lanka",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇱🇰",
	fitzpatrick_scale: false,
	category: "flags"
};
var sudan = {
	keywords: [
		"sd",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇸🇩",
	fitzpatrick_scale: false,
	category: "flags"
};
var suriname = {
	keywords: [
		"sr",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇸🇷",
	fitzpatrick_scale: false,
	category: "flags"
};
var swaziland = {
	keywords: [
		"sz",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇸🇿",
	fitzpatrick_scale: false,
	category: "flags"
};
var sweden = {
	keywords: [
		"se",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇸🇪",
	fitzpatrick_scale: false,
	category: "flags"
};
var switzerland = {
	keywords: [
		"ch",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇨🇭",
	fitzpatrick_scale: false,
	category: "flags"
};
var syria = {
	keywords: [
		"syrian",
		"arab",
		"republic",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇸🇾",
	fitzpatrick_scale: false,
	category: "flags"
};
var taiwan = {
	keywords: [
		"tw",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇹🇼",
	fitzpatrick_scale: false,
	category: "flags"
};
var tajikistan = {
	keywords: [
		"tj",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇹🇯",
	fitzpatrick_scale: false,
	category: "flags"
};
var tanzania = {
	keywords: [
		"tanzania,",
		"united",
		"republic",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇹🇿",
	fitzpatrick_scale: false,
	category: "flags"
};
var thailand = {
	keywords: [
		"th",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇹🇭",
	fitzpatrick_scale: false,
	category: "flags"
};
var timor_leste = {
	keywords: [
		"timor",
		"leste",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇹🇱",
	fitzpatrick_scale: false,
	category: "flags"
};
var togo = {
	keywords: [
		"tg",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇹🇬",
	fitzpatrick_scale: false,
	category: "flags"
};
var tokelau = {
	keywords: [
		"tk",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇹🇰",
	fitzpatrick_scale: false,
	category: "flags"
};
var tonga = {
	keywords: [
		"to",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇹🇴",
	fitzpatrick_scale: false,
	category: "flags"
};
var trinidad_tobago = {
	keywords: [
		"trinidad",
		"tobago",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇹🇹",
	fitzpatrick_scale: false,
	category: "flags"
};
var tunisia = {
	keywords: [
		"tn",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇹🇳",
	fitzpatrick_scale: false,
	category: "flags"
};
var tr = {
	keywords: [
		"turkey",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇹🇷",
	fitzpatrick_scale: false,
	category: "flags"
};
var turkmenistan = {
	keywords: [
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇹🇲",
	fitzpatrick_scale: false,
	category: "flags"
};
var turks_caicos_islands = {
	keywords: [
		"turks",
		"caicos",
		"islands",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇹🇨",
	fitzpatrick_scale: false,
	category: "flags"
};
var tuvalu = {
	keywords: [
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇹🇻",
	fitzpatrick_scale: false,
	category: "flags"
};
var uganda = {
	keywords: [
		"ug",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇺🇬",
	fitzpatrick_scale: false,
	category: "flags"
};
var ukraine = {
	keywords: [
		"ua",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇺🇦",
	fitzpatrick_scale: false,
	category: "flags"
};
var united_arab_emirates = {
	keywords: [
		"united",
		"arab",
		"emirates",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇦🇪",
	fitzpatrick_scale: false,
	category: "flags"
};
var uk = {
	keywords: [
		"united",
		"kingdom",
		"great",
		"britain",
		"northern",
		"ireland",
		"flag",
		"nation",
		"country",
		"banner",
		"british",
		"UK",
		"english",
		"england",
		"union jack"
	],
	char: "🇬🇧",
	fitzpatrick_scale: false,
	category: "flags"
};
var england = {
	keywords: [
		"flag",
		"english"
	],
	char: "🏴󠁧󠁢󠁥󠁮󠁧󠁿",
	fitzpatrick_scale: false,
	category: "flags"
};
var scotland = {
	keywords: [
		"flag",
		"scottish"
	],
	char: "🏴󠁧󠁢󠁳󠁣󠁴󠁿",
	fitzpatrick_scale: false,
	category: "flags"
};
var wales = {
	keywords: [
		"flag",
		"welsh"
	],
	char: "🏴󠁧󠁢󠁷󠁬󠁳󠁿",
	fitzpatrick_scale: false,
	category: "flags"
};
var us = {
	keywords: [
		"united",
		"states",
		"america",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇺🇸",
	fitzpatrick_scale: false,
	category: "flags"
};
var us_virgin_islands = {
	keywords: [
		"virgin",
		"islands",
		"us",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇻🇮",
	fitzpatrick_scale: false,
	category: "flags"
};
var uruguay = {
	keywords: [
		"uy",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇺🇾",
	fitzpatrick_scale: false,
	category: "flags"
};
var uzbekistan = {
	keywords: [
		"uz",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇺🇿",
	fitzpatrick_scale: false,
	category: "flags"
};
var vanuatu = {
	keywords: [
		"vu",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇻🇺",
	fitzpatrick_scale: false,
	category: "flags"
};
var vatican_city = {
	keywords: [
		"vatican",
		"city",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇻🇦",
	fitzpatrick_scale: false,
	category: "flags"
};
var venezuela = {
	keywords: [
		"ve",
		"bolivarian",
		"republic",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇻🇪",
	fitzpatrick_scale: false,
	category: "flags"
};
var vietnam = {
	keywords: [
		"viet",
		"nam",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇻🇳",
	fitzpatrick_scale: false,
	category: "flags"
};
var wallis_futuna = {
	keywords: [
		"wallis",
		"futuna",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇼🇫",
	fitzpatrick_scale: false,
	category: "flags"
};
var western_sahara = {
	keywords: [
		"western",
		"sahara",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇪🇭",
	fitzpatrick_scale: false,
	category: "flags"
};
var yemen = {
	keywords: [
		"ye",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇾🇪",
	fitzpatrick_scale: false,
	category: "flags"
};
var zambia = {
	keywords: [
		"zm",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇿🇲",
	fitzpatrick_scale: false,
	category: "flags"
};
var zimbabwe = {
	keywords: [
		"zw",
		"flag",
		"nation",
		"country",
		"banner"
	],
	char: "🇿🇼",
	fitzpatrick_scale: false,
	category: "flags"
};
var united_nations = {
	keywords: [
		"un",
		"flag",
		"banner"
	],
	char: "🇺🇳",
	fitzpatrick_scale: false,
	category: "flags"
};
var pirate_flag = {
	keywords: [
		"skull",
		"crossbones",
		"flag",
		"banner"
	],
	char: "🏴‍☠️",
	fitzpatrick_scale: false,
	category: "flags"
};
var require$$0 = {
	"100": {
	keywords: [
		"score",
		"perfect",
		"numbers",
		"century",
		"exam",
		"quiz",
		"test",
		"pass",
		"hundred"
	],
	char: "💯",
	fitzpatrick_scale: false,
	category: "symbols"
},
	"1234": {
	keywords: [
		"numbers",
		"blue-square"
	],
	char: "🔢",
	fitzpatrick_scale: false,
	category: "symbols"
},
	grinning: grinning,
	grimacing: grimacing,
	grin: grin,
	joy: joy,
	rofl: rofl,
	partying: partying,
	smiley: smiley,
	smile: smile,
	sweat_smile: sweat_smile,
	laughing: laughing,
	innocent: innocent,
	wink: wink,
	blush: blush,
	slightly_smiling_face: slightly_smiling_face,
	upside_down_face: upside_down_face,
	relaxed: relaxed,
	yum: yum,
	relieved: relieved,
	heart_eyes: heart_eyes,
	smiling_face_with_three_hearts: smiling_face_with_three_hearts,
	kissing_heart: kissing_heart,
	kissing: kissing,
	kissing_smiling_eyes: kissing_smiling_eyes,
	kissing_closed_eyes: kissing_closed_eyes,
	stuck_out_tongue_winking_eye: stuck_out_tongue_winking_eye,
	zany: zany,
	raised_eyebrow: raised_eyebrow,
	monocle: monocle,
	stuck_out_tongue_closed_eyes: stuck_out_tongue_closed_eyes,
	stuck_out_tongue: stuck_out_tongue,
	money_mouth_face: money_mouth_face,
	nerd_face: nerd_face,
	sunglasses: sunglasses,
	star_struck: star_struck,
	clown_face: clown_face,
	cowboy_hat_face: cowboy_hat_face,
	hugs: hugs,
	smirk: smirk,
	no_mouth: no_mouth,
	neutral_face: neutral_face,
	expressionless: expressionless,
	unamused: unamused,
	roll_eyes: roll_eyes,
	thinking: thinking,
	lying_face: lying_face,
	hand_over_mouth: hand_over_mouth,
	shushing: shushing,
	symbols_over_mouth: symbols_over_mouth,
	exploding_head: exploding_head,
	flushed: flushed,
	disappointed: disappointed,
	worried: worried,
	angry: angry,
	rage: rage,
	pensive: pensive,
	confused: confused,
	slightly_frowning_face: slightly_frowning_face,
	frowning_face: frowning_face,
	persevere: persevere,
	confounded: confounded,
	tired_face: tired_face,
	weary: weary,
	pleading: pleading,
	triumph: triumph,
	open_mouth: open_mouth,
	scream: scream,
	fearful: fearful,
	cold_sweat: cold_sweat,
	hushed: hushed,
	frowning: frowning,
	anguished: anguished,
	cry: cry,
	disappointed_relieved: disappointed_relieved,
	drooling_face: drooling_face,
	sleepy: sleepy,
	sweat: sweat,
	hot: hot,
	cold: cold,
	sob: sob,
	dizzy_face: dizzy_face,
	astonished: astonished,
	zipper_mouth_face: zipper_mouth_face,
	nauseated_face: nauseated_face,
	sneezing_face: sneezing_face,
	vomiting: vomiting,
	mask: mask,
	face_with_thermometer: face_with_thermometer,
	face_with_head_bandage: face_with_head_bandage,
	woozy: woozy,
	sleeping: sleeping,
	zzz: zzz,
	poop: poop,
	smiling_imp: smiling_imp,
	imp: imp,
	japanese_ogre: japanese_ogre,
	japanese_goblin: japanese_goblin,
	skull: skull,
	ghost: ghost,
	alien: alien,
	robot: robot,
	smiley_cat: smiley_cat,
	smile_cat: smile_cat,
	joy_cat: joy_cat,
	heart_eyes_cat: heart_eyes_cat,
	smirk_cat: smirk_cat,
	kissing_cat: kissing_cat,
	scream_cat: scream_cat,
	crying_cat_face: crying_cat_face,
	pouting_cat: pouting_cat,
	palms_up: palms_up,
	raised_hands: raised_hands,
	clap: clap,
	wave: wave,
	call_me_hand: call_me_hand,
	"+1": {
	keywords: [
		"thumbsup",
		"yes",
		"awesome",
		"good",
		"agree",
		"accept",
		"cool",
		"hand",
		"like"
	],
	char: "👍",
	fitzpatrick_scale: true,
	category: "people"
},
	"-1": {
	keywords: [
		"thumbsdown",
		"no",
		"dislike",
		"hand"
	],
	char: "👎",
	fitzpatrick_scale: true,
	category: "people"
},
	facepunch: facepunch,
	fist: fist,
	fist_left: fist_left,
	fist_right: fist_right,
	v: v,
	ok_hand: ok_hand,
	raised_hand: raised_hand,
	raised_back_of_hand: raised_back_of_hand,
	open_hands: open_hands,
	muscle: muscle,
	pray: pray,
	foot: foot,
	leg: leg,
	handshake: handshake,
	point_up: point_up,
	point_up_2: point_up_2,
	point_down: point_down,
	point_left: point_left,
	point_right: point_right,
	fu: fu,
	raised_hand_with_fingers_splayed: raised_hand_with_fingers_splayed,
	love_you: love_you,
	metal: metal,
	crossed_fingers: crossed_fingers,
	vulcan_salute: vulcan_salute,
	writing_hand: writing_hand,
	selfie: selfie,
	nail_care: nail_care,
	lips: lips,
	tooth: tooth,
	tongue: tongue,
	ear: ear,
	nose: nose,
	eye: eye,
	eyes: eyes,
	brain: brain,
	bust_in_silhouette: bust_in_silhouette,
	busts_in_silhouette: busts_in_silhouette,
	speaking_head: speaking_head,
	baby: baby,
	child: child,
	boy: boy,
	girl: girl,
	adult: adult,
	man: man,
	woman: woman,
	blonde_woman: blonde_woman,
	blonde_man: blonde_man,
	bearded_person: bearded_person,
	older_adult: older_adult,
	older_man: older_man,
	older_woman: older_woman,
	man_with_gua_pi_mao: man_with_gua_pi_mao,
	woman_with_headscarf: woman_with_headscarf,
	woman_with_turban: woman_with_turban,
	man_with_turban: man_with_turban,
	policewoman: policewoman,
	policeman: policeman,
	construction_worker_woman: construction_worker_woman,
	construction_worker_man: construction_worker_man,
	guardswoman: guardswoman,
	guardsman: guardsman,
	female_detective: female_detective,
	male_detective: male_detective,
	woman_health_worker: woman_health_worker,
	man_health_worker: man_health_worker,
	woman_farmer: woman_farmer,
	man_farmer: man_farmer,
	woman_cook: woman_cook,
	man_cook: man_cook,
	woman_student: woman_student,
	man_student: man_student,
	woman_singer: woman_singer,
	man_singer: man_singer,
	woman_teacher: woman_teacher,
	man_teacher: man_teacher,
	woman_factory_worker: woman_factory_worker,
	man_factory_worker: man_factory_worker,
	woman_technologist: woman_technologist,
	man_technologist: man_technologist,
	woman_office_worker: woman_office_worker,
	man_office_worker: man_office_worker,
	woman_mechanic: woman_mechanic,
	man_mechanic: man_mechanic,
	woman_scientist: woman_scientist,
	man_scientist: man_scientist,
	woman_artist: woman_artist,
	man_artist: man_artist,
	woman_firefighter: woman_firefighter,
	man_firefighter: man_firefighter,
	woman_pilot: woman_pilot,
	man_pilot: man_pilot,
	woman_astronaut: woman_astronaut,
	man_astronaut: man_astronaut,
	woman_judge: woman_judge,
	man_judge: man_judge,
	woman_superhero: woman_superhero,
	man_superhero: man_superhero,
	woman_supervillain: woman_supervillain,
	man_supervillain: man_supervillain,
	mrs_claus: mrs_claus,
	santa: santa,
	sorceress: sorceress,
	wizard: wizard,
	woman_elf: woman_elf,
	man_elf: man_elf,
	woman_vampire: woman_vampire,
	man_vampire: man_vampire,
	woman_zombie: woman_zombie,
	man_zombie: man_zombie,
	woman_genie: woman_genie,
	man_genie: man_genie,
	mermaid: mermaid,
	merman: merman,
	woman_fairy: woman_fairy,
	man_fairy: man_fairy,
	angel: angel,
	pregnant_woman: pregnant_woman,
	breastfeeding: breastfeeding,
	princess: princess,
	prince: prince,
	bride_with_veil: bride_with_veil,
	man_in_tuxedo: man_in_tuxedo,
	running_woman: running_woman,
	running_man: running_man,
	walking_woman: walking_woman,
	walking_man: walking_man,
	dancer: dancer,
	man_dancing: man_dancing,
	dancing_women: dancing_women,
	dancing_men: dancing_men,
	couple: couple,
	two_men_holding_hands: two_men_holding_hands,
	two_women_holding_hands: two_women_holding_hands,
	bowing_woman: bowing_woman,
	bowing_man: bowing_man,
	man_facepalming: man_facepalming,
	woman_facepalming: woman_facepalming,
	woman_shrugging: woman_shrugging,
	man_shrugging: man_shrugging,
	tipping_hand_woman: tipping_hand_woman,
	tipping_hand_man: tipping_hand_man,
	no_good_woman: no_good_woman,
	no_good_man: no_good_man,
	ok_woman: ok_woman,
	ok_man: ok_man,
	raising_hand_woman: raising_hand_woman,
	raising_hand_man: raising_hand_man,
	pouting_woman: pouting_woman,
	pouting_man: pouting_man,
	frowning_woman: frowning_woman,
	frowning_man: frowning_man,
	haircut_woman: haircut_woman,
	haircut_man: haircut_man,
	massage_woman: massage_woman,
	massage_man: massage_man,
	woman_in_steamy_room: woman_in_steamy_room,
	man_in_steamy_room: man_in_steamy_room,
	couple_with_heart_woman_man: couple_with_heart_woman_man,
	couple_with_heart_woman_woman: couple_with_heart_woman_woman,
	couple_with_heart_man_man: couple_with_heart_man_man,
	couplekiss_man_woman: couplekiss_man_woman,
	couplekiss_woman_woman: couplekiss_woman_woman,
	couplekiss_man_man: couplekiss_man_man,
	family_man_woman_boy: family_man_woman_boy,
	family_man_woman_girl: family_man_woman_girl,
	family_man_woman_girl_boy: family_man_woman_girl_boy,
	family_man_woman_boy_boy: family_man_woman_boy_boy,
	family_man_woman_girl_girl: family_man_woman_girl_girl,
	family_woman_woman_boy: family_woman_woman_boy,
	family_woman_woman_girl: family_woman_woman_girl,
	family_woman_woman_girl_boy: family_woman_woman_girl_boy,
	family_woman_woman_boy_boy: family_woman_woman_boy_boy,
	family_woman_woman_girl_girl: family_woman_woman_girl_girl,
	family_man_man_boy: family_man_man_boy,
	family_man_man_girl: family_man_man_girl,
	family_man_man_girl_boy: family_man_man_girl_boy,
	family_man_man_boy_boy: family_man_man_boy_boy,
	family_man_man_girl_girl: family_man_man_girl_girl,
	family_woman_boy: family_woman_boy,
	family_woman_girl: family_woman_girl,
	family_woman_girl_boy: family_woman_girl_boy,
	family_woman_boy_boy: family_woman_boy_boy,
	family_woman_girl_girl: family_woman_girl_girl,
	family_man_boy: family_man_boy,
	family_man_girl: family_man_girl,
	family_man_girl_boy: family_man_girl_boy,
	family_man_boy_boy: family_man_boy_boy,
	family_man_girl_girl: family_man_girl_girl,
	yarn: yarn,
	thread: thread,
	coat: coat,
	labcoat: labcoat,
	womans_clothes: womans_clothes,
	tshirt: tshirt,
	jeans: jeans,
	necktie: necktie,
	dress: dress,
	bikini: bikini,
	kimono: kimono,
	lipstick: lipstick,
	kiss: kiss,
	footprints: footprints,
	flat_shoe: flat_shoe,
	high_heel: high_heel,
	sandal: sandal,
	boot: boot,
	mans_shoe: mans_shoe,
	athletic_shoe: athletic_shoe,
	hiking_boot: hiking_boot,
	socks: socks,
	gloves: gloves,
	scarf: scarf,
	womans_hat: womans_hat,
	tophat: tophat,
	billed_hat: billed_hat,
	rescue_worker_helmet: rescue_worker_helmet,
	mortar_board: mortar_board,
	crown: crown,
	school_satchel: school_satchel,
	luggage: luggage,
	pouch: pouch,
	purse: purse,
	handbag: handbag,
	briefcase: briefcase,
	eyeglasses: eyeglasses,
	dark_sunglasses: dark_sunglasses,
	goggles: goggles,
	ring: ring,
	closed_umbrella: closed_umbrella,
	dog: dog,
	cat: cat,
	mouse: mouse,
	hamster: hamster,
	rabbit: rabbit,
	fox_face: fox_face,
	bear: bear,
	panda_face: panda_face,
	koala: koala,
	tiger: tiger,
	lion: lion,
	cow: cow,
	pig: pig,
	pig_nose: pig_nose,
	frog: frog,
	squid: squid,
	octopus: octopus,
	shrimp: shrimp,
	monkey_face: monkey_face,
	gorilla: gorilla,
	see_no_evil: see_no_evil,
	hear_no_evil: hear_no_evil,
	speak_no_evil: speak_no_evil,
	monkey: monkey,
	chicken: chicken,
	penguin: penguin,
	bird: bird,
	baby_chick: baby_chick,
	hatching_chick: hatching_chick,
	hatched_chick: hatched_chick,
	duck: duck,
	eagle: eagle,
	owl: owl,
	bat: bat,
	wolf: wolf,
	boar: boar,
	horse: horse,
	unicorn: unicorn,
	honeybee: honeybee,
	bug: bug,
	butterfly: butterfly,
	snail: snail,
	beetle: beetle,
	ant: ant,
	grasshopper: grasshopper,
	spider: spider,
	scorpion: scorpion,
	crab: crab,
	snake: snake,
	lizard: lizard,
	"t-rex": {
	keywords: [
		"animal",
		"nature",
		"dinosaur",
		"tyrannosaurus",
		"extinct"
	],
	char: "🦖",
	fitzpatrick_scale: false,
	category: "animals_and_nature"
},
	sauropod: sauropod,
	turtle: turtle,
	tropical_fish: tropical_fish,
	fish: fish,
	blowfish: blowfish,
	dolphin: dolphin,
	shark: shark,
	whale: whale,
	whale2: whale2,
	crocodile: crocodile,
	leopard: leopard,
	zebra: zebra,
	tiger2: tiger2,
	water_buffalo: water_buffalo,
	ox: ox,
	cow2: cow2,
	deer: deer,
	dromedary_camel: dromedary_camel,
	camel: camel,
	giraffe: giraffe,
	elephant: elephant,
	rhinoceros: rhinoceros,
	goat: goat,
	ram: ram,
	sheep: sheep,
	racehorse: racehorse,
	pig2: pig2,
	rat: rat,
	mouse2: mouse2,
	rooster: rooster,
	turkey: turkey,
	dove: dove,
	dog2: dog2,
	poodle: poodle,
	cat2: cat2,
	rabbit2: rabbit2,
	chipmunk: chipmunk,
	hedgehog: hedgehog,
	raccoon: raccoon,
	llama: llama,
	hippopotamus: hippopotamus,
	kangaroo: kangaroo,
	badger: badger,
	swan: swan,
	peacock: peacock,
	parrot: parrot,
	lobster: lobster,
	mosquito: mosquito,
	paw_prints: paw_prints,
	dragon: dragon,
	dragon_face: dragon_face,
	cactus: cactus,
	christmas_tree: christmas_tree,
	evergreen_tree: evergreen_tree,
	deciduous_tree: deciduous_tree,
	palm_tree: palm_tree,
	seedling: seedling,
	herb: herb,
	shamrock: shamrock,
	four_leaf_clover: four_leaf_clover,
	bamboo: bamboo,
	tanabata_tree: tanabata_tree,
	leaves: leaves,
	fallen_leaf: fallen_leaf,
	maple_leaf: maple_leaf,
	ear_of_rice: ear_of_rice,
	hibiscus: hibiscus,
	sunflower: sunflower,
	rose: rose,
	wilted_flower: wilted_flower,
	tulip: tulip,
	blossom: blossom,
	cherry_blossom: cherry_blossom,
	bouquet: bouquet,
	mushroom: mushroom,
	chestnut: chestnut,
	jack_o_lantern: jack_o_lantern,
	shell: shell,
	spider_web: spider_web,
	earth_americas: earth_americas,
	earth_africa: earth_africa,
	earth_asia: earth_asia,
	full_moon: full_moon,
	waning_gibbous_moon: waning_gibbous_moon,
	last_quarter_moon: last_quarter_moon,
	waning_crescent_moon: waning_crescent_moon,
	new_moon: new_moon,
	waxing_crescent_moon: waxing_crescent_moon,
	first_quarter_moon: first_quarter_moon,
	waxing_gibbous_moon: waxing_gibbous_moon,
	new_moon_with_face: new_moon_with_face,
	full_moon_with_face: full_moon_with_face,
	first_quarter_moon_with_face: first_quarter_moon_with_face,
	last_quarter_moon_with_face: last_quarter_moon_with_face,
	sun_with_face: sun_with_face,
	crescent_moon: crescent_moon,
	star: star,
	star2: star2,
	dizzy: dizzy,
	sparkles: sparkles,
	comet: comet,
	sunny: sunny,
	sun_behind_small_cloud: sun_behind_small_cloud,
	partly_sunny: partly_sunny,
	sun_behind_large_cloud: sun_behind_large_cloud,
	sun_behind_rain_cloud: sun_behind_rain_cloud,
	cloud: cloud,
	cloud_with_rain: cloud_with_rain,
	cloud_with_lightning_and_rain: cloud_with_lightning_and_rain,
	cloud_with_lightning: cloud_with_lightning,
	zap: zap,
	fire: fire,
	boom: boom,
	snowflake: snowflake,
	cloud_with_snow: cloud_with_snow,
	snowman: snowman,
	snowman_with_snow: snowman_with_snow,
	wind_face: wind_face,
	dash: dash,
	tornado: tornado,
	fog: fog,
	open_umbrella: open_umbrella,
	umbrella: umbrella,
	droplet: droplet,
	sweat_drops: sweat_drops,
	ocean: ocean,
	green_apple: green_apple,
	apple: apple,
	pear: pear,
	tangerine: tangerine,
	lemon: lemon,
	banana: banana,
	watermelon: watermelon,
	grapes: grapes,
	strawberry: strawberry,
	melon: melon,
	cherries: cherries,
	peach: peach,
	pineapple: pineapple,
	coconut: coconut,
	kiwi_fruit: kiwi_fruit,
	mango: mango,
	avocado: avocado,
	broccoli: broccoli,
	tomato: tomato,
	eggplant: eggplant,
	cucumber: cucumber,
	carrot: carrot,
	hot_pepper: hot_pepper,
	potato: potato,
	corn: corn,
	leafy_greens: leafy_greens,
	sweet_potato: sweet_potato,
	peanuts: peanuts,
	honey_pot: honey_pot,
	croissant: croissant,
	bread: bread,
	baguette_bread: baguette_bread,
	bagel: bagel,
	pretzel: pretzel,
	cheese: cheese,
	egg: egg,
	bacon: bacon,
	steak: steak,
	pancakes: pancakes,
	poultry_leg: poultry_leg,
	meat_on_bone: meat_on_bone,
	bone: bone,
	fried_shrimp: fried_shrimp,
	fried_egg: fried_egg,
	hamburger: hamburger,
	fries: fries,
	stuffed_flatbread: stuffed_flatbread,
	hotdog: hotdog,
	pizza: pizza,
	sandwich: sandwich,
	canned_food: canned_food,
	spaghetti: spaghetti,
	taco: taco,
	burrito: burrito,
	green_salad: green_salad,
	shallow_pan_of_food: shallow_pan_of_food,
	ramen: ramen,
	stew: stew,
	fish_cake: fish_cake,
	fortune_cookie: fortune_cookie,
	sushi: sushi,
	bento: bento,
	curry: curry,
	rice_ball: rice_ball,
	rice: rice,
	rice_cracker: rice_cracker,
	oden: oden,
	dango: dango,
	shaved_ice: shaved_ice,
	ice_cream: ice_cream,
	icecream: icecream,
	pie: pie,
	cake: cake,
	cupcake: cupcake,
	moon_cake: moon_cake,
	birthday: birthday,
	custard: custard,
	candy: candy,
	lollipop: lollipop,
	chocolate_bar: chocolate_bar,
	popcorn: popcorn,
	dumpling: dumpling,
	doughnut: doughnut,
	cookie: cookie,
	milk_glass: milk_glass,
	beer: beer,
	beers: beers,
	clinking_glasses: clinking_glasses,
	wine_glass: wine_glass,
	tumbler_glass: tumbler_glass,
	cocktail: cocktail,
	tropical_drink: tropical_drink,
	champagne: champagne,
	sake: sake,
	tea: tea,
	cup_with_straw: cup_with_straw,
	coffee: coffee,
	baby_bottle: baby_bottle,
	salt: salt,
	spoon: spoon,
	fork_and_knife: fork_and_knife,
	plate_with_cutlery: plate_with_cutlery,
	bowl_with_spoon: bowl_with_spoon,
	takeout_box: takeout_box,
	chopsticks: chopsticks,
	soccer: soccer,
	basketball: basketball,
	football: football,
	baseball: baseball,
	softball: softball,
	tennis: tennis,
	volleyball: volleyball,
	rugby_football: rugby_football,
	flying_disc: flying_disc,
	"8ball": {
	keywords: [
		"pool",
		"hobby",
		"game",
		"luck",
		"magic"
	],
	char: "🎱",
	fitzpatrick_scale: false,
	category: "activity"
},
	golf: golf,
	golfing_woman: golfing_woman,
	golfing_man: golfing_man,
	ping_pong: ping_pong,
	badminton: badminton,
	goal_net: goal_net,
	ice_hockey: ice_hockey,
	field_hockey: field_hockey,
	lacrosse: lacrosse,
	cricket: cricket,
	ski: ski,
	skier: skier,
	snowboarder: snowboarder,
	person_fencing: person_fencing,
	women_wrestling: women_wrestling,
	men_wrestling: men_wrestling,
	woman_cartwheeling: woman_cartwheeling,
	man_cartwheeling: man_cartwheeling,
	woman_playing_handball: woman_playing_handball,
	man_playing_handball: man_playing_handball,
	ice_skate: ice_skate,
	curling_stone: curling_stone,
	skateboard: skateboard,
	sled: sled,
	bow_and_arrow: bow_and_arrow,
	fishing_pole_and_fish: fishing_pole_and_fish,
	boxing_glove: boxing_glove,
	martial_arts_uniform: martial_arts_uniform,
	rowing_woman: rowing_woman,
	rowing_man: rowing_man,
	climbing_woman: climbing_woman,
	climbing_man: climbing_man,
	swimming_woman: swimming_woman,
	swimming_man: swimming_man,
	woman_playing_water_polo: woman_playing_water_polo,
	man_playing_water_polo: man_playing_water_polo,
	woman_in_lotus_position: woman_in_lotus_position,
	man_in_lotus_position: man_in_lotus_position,
	surfing_woman: surfing_woman,
	surfing_man: surfing_man,
	bath: bath,
	basketball_woman: basketball_woman,
	basketball_man: basketball_man,
	weight_lifting_woman: weight_lifting_woman,
	weight_lifting_man: weight_lifting_man,
	biking_woman: biking_woman,
	biking_man: biking_man,
	mountain_biking_woman: mountain_biking_woman,
	mountain_biking_man: mountain_biking_man,
	horse_racing: horse_racing,
	business_suit_levitating: business_suit_levitating,
	trophy: trophy,
	running_shirt_with_sash: running_shirt_with_sash,
	medal_sports: medal_sports,
	medal_military: medal_military,
	"1st_place_medal": {
	keywords: [
		"award",
		"winning",
		"first"
	],
	char: "🥇",
	fitzpatrick_scale: false,
	category: "activity"
},
	"2nd_place_medal": {
	keywords: [
		"award",
		"second"
	],
	char: "🥈",
	fitzpatrick_scale: false,
	category: "activity"
},
	"3rd_place_medal": {
	keywords: [
		"award",
		"third"
	],
	char: "🥉",
	fitzpatrick_scale: false,
	category: "activity"
},
	reminder_ribbon: reminder_ribbon,
	rosette: rosette,
	ticket: ticket,
	tickets: tickets,
	performing_arts: performing_arts,
	art: art,
	circus_tent: circus_tent,
	woman_juggling: woman_juggling,
	man_juggling: man_juggling,
	microphone: microphone,
	headphones: headphones,
	musical_score: musical_score,
	musical_keyboard: musical_keyboard,
	drum: drum,
	saxophone: saxophone,
	trumpet: trumpet,
	guitar: guitar,
	violin: violin,
	clapper: clapper,
	video_game: video_game,
	space_invader: space_invader,
	dart: dart,
	game_die: game_die,
	chess_pawn: chess_pawn,
	slot_machine: slot_machine,
	jigsaw: jigsaw,
	bowling: bowling,
	red_car: red_car,
	taxi: taxi,
	blue_car: blue_car,
	bus: bus,
	trolleybus: trolleybus,
	racing_car: racing_car,
	police_car: police_car,
	ambulance: ambulance,
	fire_engine: fire_engine,
	minibus: minibus,
	truck: truck,
	articulated_lorry: articulated_lorry,
	tractor: tractor,
	kick_scooter: kick_scooter,
	motorcycle: motorcycle,
	bike: bike,
	motor_scooter: motor_scooter,
	rotating_light: rotating_light,
	oncoming_police_car: oncoming_police_car,
	oncoming_bus: oncoming_bus,
	oncoming_automobile: oncoming_automobile,
	oncoming_taxi: oncoming_taxi,
	aerial_tramway: aerial_tramway,
	mountain_cableway: mountain_cableway,
	suspension_railway: suspension_railway,
	railway_car: railway_car,
	train: train,
	monorail: monorail,
	bullettrain_side: bullettrain_side,
	bullettrain_front: bullettrain_front,
	light_rail: light_rail,
	mountain_railway: mountain_railway,
	steam_locomotive: steam_locomotive,
	train2: train2,
	metro: metro,
	tram: tram,
	station: station,
	flying_saucer: flying_saucer,
	helicopter: helicopter,
	small_airplane: small_airplane,
	airplane: airplane,
	flight_departure: flight_departure,
	flight_arrival: flight_arrival,
	sailboat: sailboat,
	motor_boat: motor_boat,
	speedboat: speedboat,
	ferry: ferry,
	passenger_ship: passenger_ship,
	rocket: rocket,
	artificial_satellite: artificial_satellite,
	seat: seat,
	canoe: canoe,
	anchor: anchor,
	construction: construction,
	fuelpump: fuelpump,
	busstop: busstop,
	vertical_traffic_light: vertical_traffic_light,
	traffic_light: traffic_light,
	checkered_flag: checkered_flag,
	ship: ship,
	ferris_wheel: ferris_wheel,
	roller_coaster: roller_coaster,
	carousel_horse: carousel_horse,
	building_construction: building_construction,
	foggy: foggy,
	tokyo_tower: tokyo_tower,
	factory: factory,
	fountain: fountain,
	rice_scene: rice_scene,
	mountain: mountain,
	mountain_snow: mountain_snow,
	mount_fuji: mount_fuji,
	volcano: volcano,
	japan: japan,
	camping: camping,
	tent: tent,
	national_park: national_park,
	motorway: motorway,
	railway_track: railway_track,
	sunrise: sunrise,
	sunrise_over_mountains: sunrise_over_mountains,
	desert: desert,
	beach_umbrella: beach_umbrella,
	desert_island: desert_island,
	city_sunrise: city_sunrise,
	city_sunset: city_sunset,
	cityscape: cityscape,
	night_with_stars: night_with_stars,
	bridge_at_night: bridge_at_night,
	milky_way: milky_way,
	stars: stars,
	sparkler: sparkler,
	fireworks: fireworks,
	rainbow: rainbow,
	houses: houses,
	european_castle: european_castle,
	japanese_castle: japanese_castle,
	stadium: stadium,
	statue_of_liberty: statue_of_liberty,
	house: house,
	house_with_garden: house_with_garden,
	derelict_house: derelict_house,
	office: office,
	department_store: department_store,
	post_office: post_office,
	european_post_office: european_post_office,
	hospital: hospital,
	bank: bank,
	hotel: hotel,
	convenience_store: convenience_store,
	school: school,
	love_hotel: love_hotel,
	wedding: wedding,
	classical_building: classical_building,
	church: church,
	mosque: mosque,
	synagogue: synagogue,
	kaaba: kaaba,
	shinto_shrine: shinto_shrine,
	watch: watch,
	iphone: iphone,
	calling: calling,
	computer: computer,
	keyboard: keyboard,
	desktop_computer: desktop_computer,
	printer: printer,
	computer_mouse: computer_mouse,
	trackball: trackball,
	joystick: joystick,
	clamp: clamp,
	minidisc: minidisc,
	floppy_disk: floppy_disk,
	cd: cd,
	dvd: dvd,
	vhs: vhs,
	camera: camera,
	camera_flash: camera_flash,
	video_camera: video_camera,
	movie_camera: movie_camera,
	film_projector: film_projector,
	film_strip: film_strip,
	telephone_receiver: telephone_receiver,
	phone: phone,
	pager: pager,
	fax: fax,
	tv: tv,
	radio: radio,
	studio_microphone: studio_microphone,
	level_slider: level_slider,
	control_knobs: control_knobs,
	compass: compass,
	stopwatch: stopwatch,
	timer_clock: timer_clock,
	alarm_clock: alarm_clock,
	mantelpiece_clock: mantelpiece_clock,
	hourglass_flowing_sand: hourglass_flowing_sand,
	hourglass: hourglass,
	satellite: satellite,
	battery: battery,
	electric_plug: electric_plug,
	bulb: bulb,
	flashlight: flashlight,
	candle: candle,
	fire_extinguisher: fire_extinguisher,
	wastebasket: wastebasket,
	oil_drum: oil_drum,
	money_with_wings: money_with_wings,
	dollar: dollar,
	yen: yen,
	euro: euro,
	pound: pound,
	moneybag: moneybag,
	credit_card: credit_card,
	gem: gem,
	balance_scale: balance_scale,
	toolbox: toolbox,
	wrench: wrench,
	hammer: hammer,
	hammer_and_pick: hammer_and_pick,
	hammer_and_wrench: hammer_and_wrench,
	pick: pick,
	nut_and_bolt: nut_and_bolt,
	gear: gear,
	brick: brick,
	chains: chains,
	magnet: magnet,
	gun: gun,
	bomb: bomb,
	firecracker: firecracker,
	hocho: hocho,
	dagger: dagger,
	crossed_swords: crossed_swords,
	shield: shield,
	smoking: smoking,
	skull_and_crossbones: skull_and_crossbones,
	coffin: coffin,
	funeral_urn: funeral_urn,
	amphora: amphora,
	crystal_ball: crystal_ball,
	prayer_beads: prayer_beads,
	nazar_amulet: nazar_amulet,
	barber: barber,
	alembic: alembic,
	telescope: telescope,
	microscope: microscope,
	hole: hole,
	pill: pill,
	syringe: syringe,
	dna: dna,
	microbe: microbe,
	petri_dish: petri_dish,
	test_tube: test_tube,
	thermometer: thermometer,
	broom: broom,
	basket: basket,
	toilet_paper: toilet_paper,
	label: label,
	bookmark: bookmark,
	toilet: toilet,
	shower: shower,
	bathtub: bathtub,
	soap: soap,
	sponge: sponge,
	lotion_bottle: lotion_bottle,
	key: key,
	old_key: old_key,
	couch_and_lamp: couch_and_lamp,
	sleeping_bed: sleeping_bed,
	bed: bed,
	door: door,
	bellhop_bell: bellhop_bell,
	teddy_bear: teddy_bear,
	framed_picture: framed_picture,
	world_map: world_map,
	parasol_on_ground: parasol_on_ground,
	moyai: moyai,
	shopping: shopping,
	shopping_cart: shopping_cart,
	balloon: balloon,
	flags: flags,
	ribbon: ribbon,
	gift: gift,
	confetti_ball: confetti_ball,
	tada: tada,
	dolls: dolls,
	wind_chime: wind_chime,
	crossed_flags: crossed_flags,
	izakaya_lantern: izakaya_lantern,
	red_envelope: red_envelope,
	email: email,
	envelope_with_arrow: envelope_with_arrow,
	incoming_envelope: incoming_envelope,
	"e-mail": {
	keywords: [
		"communication",
		"inbox"
	],
	char: "📧",
	fitzpatrick_scale: false,
	category: "objects"
},
	love_letter: love_letter,
	postbox: postbox,
	mailbox_closed: mailbox_closed,
	mailbox: mailbox,
	mailbox_with_mail: mailbox_with_mail,
	mailbox_with_no_mail: mailbox_with_no_mail,
	"package": {
	keywords: [
		"mail",
		"gift",
		"cardboard",
		"box",
		"moving"
	],
	char: "📦",
	fitzpatrick_scale: false,
	category: "objects"
},
	postal_horn: postal_horn,
	inbox_tray: inbox_tray,
	outbox_tray: outbox_tray,
	scroll: scroll,
	page_with_curl: page_with_curl,
	bookmark_tabs: bookmark_tabs,
	receipt: receipt,
	bar_chart: bar_chart,
	chart_with_upwards_trend: chart_with_upwards_trend,
	chart_with_downwards_trend: chart_with_downwards_trend,
	page_facing_up: page_facing_up,
	date: date,
	calendar: calendar,
	spiral_calendar: spiral_calendar,
	card_index: card_index,
	card_file_box: card_file_box,
	ballot_box: ballot_box,
	file_cabinet: file_cabinet,
	clipboard: clipboard,
	spiral_notepad: spiral_notepad,
	file_folder: file_folder,
	open_file_folder: open_file_folder,
	card_index_dividers: card_index_dividers,
	newspaper_roll: newspaper_roll,
	newspaper: newspaper,
	notebook: notebook,
	closed_book: closed_book,
	green_book: green_book,
	blue_book: blue_book,
	orange_book: orange_book,
	notebook_with_decorative_cover: notebook_with_decorative_cover,
	ledger: ledger,
	books: books,
	open_book: open_book,
	safety_pin: safety_pin,
	link: link,
	paperclip: paperclip,
	paperclips: paperclips,
	scissors: scissors,
	triangular_ruler: triangular_ruler,
	straight_ruler: straight_ruler,
	abacus: abacus,
	pushpin: pushpin,
	round_pushpin: round_pushpin,
	triangular_flag_on_post: triangular_flag_on_post,
	white_flag: white_flag,
	black_flag: black_flag,
	rainbow_flag: rainbow_flag,
	closed_lock_with_key: closed_lock_with_key,
	lock: lock,
	unlock: unlock,
	lock_with_ink_pen: lock_with_ink_pen,
	pen: pen,
	fountain_pen: fountain_pen,
	black_nib: black_nib,
	memo: memo,
	pencil2: pencil2,
	crayon: crayon,
	paintbrush: paintbrush,
	mag: mag,
	mag_right: mag_right,
	heart: heart,
	orange_heart: orange_heart,
	yellow_heart: yellow_heart,
	green_heart: green_heart,
	blue_heart: blue_heart,
	purple_heart: purple_heart,
	black_heart: black_heart,
	broken_heart: broken_heart,
	heavy_heart_exclamation: heavy_heart_exclamation,
	two_hearts: two_hearts,
	revolving_hearts: revolving_hearts,
	heartbeat: heartbeat,
	heartpulse: heartpulse,
	sparkling_heart: sparkling_heart,
	cupid: cupid,
	gift_heart: gift_heart,
	heart_decoration: heart_decoration,
	peace_symbol: peace_symbol,
	latin_cross: latin_cross,
	star_and_crescent: star_and_crescent,
	om: om,
	wheel_of_dharma: wheel_of_dharma,
	star_of_david: star_of_david,
	six_pointed_star: six_pointed_star,
	menorah: menorah,
	yin_yang: yin_yang,
	orthodox_cross: orthodox_cross,
	place_of_worship: place_of_worship,
	ophiuchus: ophiuchus,
	aries: aries,
	taurus: taurus,
	gemini: gemini,
	cancer: cancer,
	leo: leo,
	virgo: virgo,
	libra: libra,
	scorpius: scorpius,
	sagittarius: sagittarius,
	capricorn: capricorn,
	aquarius: aquarius,
	pisces: pisces,
	id: id,
	atom_symbol: atom_symbol,
	u7a7a: u7a7a,
	u5272: u5272,
	radioactive: radioactive,
	biohazard: biohazard,
	mobile_phone_off: mobile_phone_off,
	vibration_mode: vibration_mode,
	u6709: u6709,
	u7121: u7121,
	u7533: u7533,
	u55b6: u55b6,
	u6708: u6708,
	eight_pointed_black_star: eight_pointed_black_star,
	vs: vs,
	accept: accept,
	white_flower: white_flower,
	ideograph_advantage: ideograph_advantage,
	secret: secret,
	congratulations: congratulations,
	u5408: u5408,
	u6e80: u6e80,
	u7981: u7981,
	a: a,
	b: b,
	ab: ab,
	cl: cl,
	o2: o2,
	sos: sos,
	no_entry: no_entry,
	name_badge: name_badge,
	no_entry_sign: no_entry_sign,
	x: x,
	o: o,
	stop_sign: stop_sign,
	anger: anger,
	hotsprings: hotsprings,
	no_pedestrians: no_pedestrians,
	do_not_litter: do_not_litter,
	no_bicycles: no_bicycles,
	"non-potable_water": {
	keywords: [
		"drink",
		"faucet",
		"tap",
		"circle"
	],
	char: "🚱",
	fitzpatrick_scale: false,
	category: "symbols"
},
	underage: underage,
	no_mobile_phones: no_mobile_phones,
	exclamation: exclamation,
	grey_exclamation: grey_exclamation,
	question: question,
	grey_question: grey_question,
	bangbang: bangbang,
	interrobang: interrobang,
	low_brightness: low_brightness,
	high_brightness: high_brightness,
	trident: trident,
	fleur_de_lis: fleur_de_lis,
	part_alternation_mark: part_alternation_mark,
	warning: warning,
	children_crossing: children_crossing,
	beginner: beginner,
	recycle: recycle,
	u6307: u6307,
	chart: chart,
	sparkle: sparkle,
	eight_spoked_asterisk: eight_spoked_asterisk,
	negative_squared_cross_mark: negative_squared_cross_mark,
	white_check_mark: white_check_mark,
	diamond_shape_with_a_dot_inside: diamond_shape_with_a_dot_inside,
	cyclone: cyclone,
	loop: loop,
	globe_with_meridians: globe_with_meridians,
	m: m,
	atm: atm,
	sa: sa,
	passport_control: passport_control,
	customs: customs,
	baggage_claim: baggage_claim,
	left_luggage: left_luggage,
	wheelchair: wheelchair,
	no_smoking: no_smoking,
	wc: wc,
	parking: parking,
	potable_water: potable_water,
	mens: mens,
	womens: womens,
	baby_symbol: baby_symbol,
	restroom: restroom,
	put_litter_in_its_place: put_litter_in_its_place,
	cinema: cinema,
	signal_strength: signal_strength,
	koko: koko,
	ng: ng,
	ok: ok,
	up: up,
	cool: cool,
	"new": {
	keywords: [
		"blue-square",
		"words",
		"start"
	],
	char: "🆕",
	fitzpatrick_scale: false,
	category: "symbols"
},
	free: free,
	zero: zero,
	one: one,
	two: two,
	three: three,
	four: four,
	five: five,
	six: six,
	seven: seven,
	eight: eight,
	nine: nine,
	keycap_ten: keycap_ten,
	asterisk: asterisk,
	eject_button: eject_button,
	arrow_forward: arrow_forward,
	pause_button: pause_button,
	next_track_button: next_track_button,
	stop_button: stop_button,
	record_button: record_button,
	play_or_pause_button: play_or_pause_button,
	previous_track_button: previous_track_button,
	fast_forward: fast_forward,
	rewind: rewind,
	twisted_rightwards_arrows: twisted_rightwards_arrows,
	repeat: repeat,
	repeat_one: repeat_one,
	arrow_backward: arrow_backward,
	arrow_up_small: arrow_up_small,
	arrow_down_small: arrow_down_small,
	arrow_double_up: arrow_double_up,
	arrow_double_down: arrow_double_down,
	arrow_right: arrow_right,
	arrow_left: arrow_left,
	arrow_up: arrow_up,
	arrow_down: arrow_down,
	arrow_upper_right: arrow_upper_right,
	arrow_lower_right: arrow_lower_right,
	arrow_lower_left: arrow_lower_left,
	arrow_upper_left: arrow_upper_left,
	arrow_up_down: arrow_up_down,
	left_right_arrow: left_right_arrow,
	arrows_counterclockwise: arrows_counterclockwise,
	arrow_right_hook: arrow_right_hook,
	leftwards_arrow_with_hook: leftwards_arrow_with_hook,
	arrow_heading_up: arrow_heading_up,
	arrow_heading_down: arrow_heading_down,
	hash: hash,
	information_source: information_source,
	abc: abc,
	abcd: abcd,
	capital_abcd: capital_abcd,
	symbols: symbols,
	musical_note: musical_note,
	notes: notes,
	wavy_dash: wavy_dash,
	curly_loop: curly_loop,
	heavy_check_mark: heavy_check_mark,
	arrows_clockwise: arrows_clockwise,
	heavy_plus_sign: heavy_plus_sign,
	heavy_minus_sign: heavy_minus_sign,
	heavy_division_sign: heavy_division_sign,
	heavy_multiplication_x: heavy_multiplication_x,
	infinity: infinity,
	heavy_dollar_sign: heavy_dollar_sign,
	currency_exchange: currency_exchange,
	copyright: copyright,
	registered: registered,
	tm: tm,
	end: end,
	back: back,
	on: on,
	top: top,
	soon: soon,
	ballot_box_with_check: ballot_box_with_check,
	radio_button: radio_button,
	white_circle: white_circle,
	black_circle: black_circle,
	red_circle: red_circle,
	large_blue_circle: large_blue_circle,
	small_orange_diamond: small_orange_diamond,
	small_blue_diamond: small_blue_diamond,
	large_orange_diamond: large_orange_diamond,
	large_blue_diamond: large_blue_diamond,
	small_red_triangle: small_red_triangle,
	black_small_square: black_small_square,
	white_small_square: white_small_square,
	black_large_square: black_large_square,
	white_large_square: white_large_square,
	small_red_triangle_down: small_red_triangle_down,
	black_medium_square: black_medium_square,
	white_medium_square: white_medium_square,
	black_medium_small_square: black_medium_small_square,
	white_medium_small_square: white_medium_small_square,
	black_square_button: black_square_button,
	white_square_button: white_square_button,
	speaker: speaker,
	sound: sound,
	loud_sound: loud_sound,
	mute: mute,
	mega: mega,
	loudspeaker: loudspeaker,
	bell: bell,
	no_bell: no_bell,
	black_joker: black_joker,
	mahjong: mahjong,
	spades: spades,
	clubs: clubs,
	hearts: hearts,
	diamonds: diamonds,
	flower_playing_cards: flower_playing_cards,
	thought_balloon: thought_balloon,
	right_anger_bubble: right_anger_bubble,
	speech_balloon: speech_balloon,
	left_speech_bubble: left_speech_bubble,
	clock1: clock1,
	clock2: clock2,
	clock3: clock3,
	clock4: clock4,
	clock5: clock5,
	clock6: clock6,
	clock7: clock7,
	clock8: clock8,
	clock9: clock9,
	clock10: clock10,
	clock11: clock11,
	clock12: clock12,
	clock130: clock130,
	clock230: clock230,
	clock330: clock330,
	clock430: clock430,
	clock530: clock530,
	clock630: clock630,
	clock730: clock730,
	clock830: clock830,
	clock930: clock930,
	clock1030: clock1030,
	clock1130: clock1130,
	clock1230: clock1230,
	afghanistan: afghanistan,
	aland_islands: aland_islands,
	albania: albania,
	algeria: algeria,
	american_samoa: american_samoa,
	andorra: andorra,
	angola: angola,
	anguilla: anguilla,
	antarctica: antarctica,
	antigua_barbuda: antigua_barbuda,
	argentina: argentina,
	armenia: armenia,
	aruba: aruba,
	australia: australia,
	austria: austria,
	azerbaijan: azerbaijan,
	bahamas: bahamas,
	bahrain: bahrain,
	bangladesh: bangladesh,
	barbados: barbados,
	belarus: belarus,
	belgium: belgium,
	belize: belize,
	benin: benin,
	bermuda: bermuda,
	bhutan: bhutan,
	bolivia: bolivia,
	caribbean_netherlands: caribbean_netherlands,
	bosnia_herzegovina: bosnia_herzegovina,
	botswana: botswana,
	brazil: brazil,
	british_indian_ocean_territory: british_indian_ocean_territory,
	british_virgin_islands: british_virgin_islands,
	brunei: brunei,
	bulgaria: bulgaria,
	burkina_faso: burkina_faso,
	burundi: burundi,
	cape_verde: cape_verde,
	cambodia: cambodia,
	cameroon: cameroon,
	canada: canada,
	canary_islands: canary_islands,
	cayman_islands: cayman_islands,
	central_african_republic: central_african_republic,
	chad: chad,
	chile: chile,
	cn: cn,
	christmas_island: christmas_island,
	cocos_islands: cocos_islands,
	colombia: colombia,
	comoros: comoros,
	congo_brazzaville: congo_brazzaville,
	congo_kinshasa: congo_kinshasa,
	cook_islands: cook_islands,
	costa_rica: costa_rica,
	croatia: croatia,
	cuba: cuba,
	curacao: curacao,
	cyprus: cyprus,
	czech_republic: czech_republic,
	denmark: denmark,
	djibouti: djibouti,
	dominica: dominica,
	dominican_republic: dominican_republic,
	ecuador: ecuador,
	egypt: egypt,
	el_salvador: el_salvador,
	equatorial_guinea: equatorial_guinea,
	eritrea: eritrea,
	estonia: estonia,
	ethiopia: ethiopia,
	eu: eu,
	falkland_islands: falkland_islands,
	faroe_islands: faroe_islands,
	fiji: fiji,
	finland: finland,
	fr: fr,
	french_guiana: french_guiana,
	french_polynesia: french_polynesia,
	french_southern_territories: french_southern_territories,
	gabon: gabon,
	gambia: gambia,
	georgia: georgia,
	de: de,
	ghana: ghana,
	gibraltar: gibraltar,
	greece: greece,
	greenland: greenland,
	grenada: grenada,
	guadeloupe: guadeloupe,
	guam: guam,
	guatemala: guatemala,
	guernsey: guernsey,
	guinea: guinea,
	guinea_bissau: guinea_bissau,
	guyana: guyana,
	haiti: haiti,
	honduras: honduras,
	hong_kong: hong_kong,
	hungary: hungary,
	iceland: iceland,
	india: india,
	indonesia: indonesia,
	iran: iran,
	iraq: iraq,
	ireland: ireland,
	isle_of_man: isle_of_man,
	israel: israel,
	it: it,
	cote_divoire: cote_divoire,
	jamaica: jamaica,
	jp: jp,
	jersey: jersey,
	jordan: jordan,
	kazakhstan: kazakhstan,
	kenya: kenya,
	kiribati: kiribati,
	kosovo: kosovo,
	kuwait: kuwait,
	kyrgyzstan: kyrgyzstan,
	laos: laos,
	latvia: latvia,
	lebanon: lebanon,
	lesotho: lesotho,
	liberia: liberia,
	libya: libya,
	liechtenstein: liechtenstein,
	lithuania: lithuania,
	luxembourg: luxembourg,
	macau: macau,
	macedonia: macedonia,
	madagascar: madagascar,
	malawi: malawi,
	malaysia: malaysia,
	maldives: maldives,
	mali: mali,
	malta: malta,
	marshall_islands: marshall_islands,
	martinique: martinique,
	mauritania: mauritania,
	mauritius: mauritius,
	mayotte: mayotte,
	mexico: mexico,
	micronesia: micronesia,
	moldova: moldova,
	monaco: monaco,
	mongolia: mongolia,
	montenegro: montenegro,
	montserrat: montserrat,
	morocco: morocco,
	mozambique: mozambique,
	myanmar: myanmar,
	namibia: namibia,
	nauru: nauru,
	nepal: nepal,
	netherlands: netherlands,
	new_caledonia: new_caledonia,
	new_zealand: new_zealand,
	nicaragua: nicaragua,
	niger: niger,
	nigeria: nigeria,
	niue: niue,
	norfolk_island: norfolk_island,
	northern_mariana_islands: northern_mariana_islands,
	north_korea: north_korea,
	norway: norway,
	oman: oman,
	pakistan: pakistan,
	palau: palau,
	palestinian_territories: palestinian_territories,
	panama: panama,
	papua_new_guinea: papua_new_guinea,
	paraguay: paraguay,
	peru: peru,
	philippines: philippines,
	pitcairn_islands: pitcairn_islands,
	poland: poland,
	portugal: portugal,
	puerto_rico: puerto_rico,
	qatar: qatar,
	reunion: reunion,
	romania: romania,
	ru: ru,
	rwanda: rwanda,
	st_barthelemy: st_barthelemy,
	st_helena: st_helena,
	st_kitts_nevis: st_kitts_nevis,
	st_lucia: st_lucia,
	st_pierre_miquelon: st_pierre_miquelon,
	st_vincent_grenadines: st_vincent_grenadines,
	samoa: samoa,
	san_marino: san_marino,
	sao_tome_principe: sao_tome_principe,
	saudi_arabia: saudi_arabia,
	senegal: senegal,
	serbia: serbia,
	seychelles: seychelles,
	sierra_leone: sierra_leone,
	singapore: singapore,
	sint_maarten: sint_maarten,
	slovakia: slovakia,
	slovenia: slovenia,
	solomon_islands: solomon_islands,
	somalia: somalia,
	south_africa: south_africa,
	south_georgia_south_sandwich_islands: south_georgia_south_sandwich_islands,
	kr: kr,
	south_sudan: south_sudan,
	es: es,
	sri_lanka: sri_lanka,
	sudan: sudan,
	suriname: suriname,
	swaziland: swaziland,
	sweden: sweden,
	switzerland: switzerland,
	syria: syria,
	taiwan: taiwan,
	tajikistan: tajikistan,
	tanzania: tanzania,
	thailand: thailand,
	timor_leste: timor_leste,
	togo: togo,
	tokelau: tokelau,
	tonga: tonga,
	trinidad_tobago: trinidad_tobago,
	tunisia: tunisia,
	tr: tr,
	turkmenistan: turkmenistan,
	turks_caicos_islands: turks_caicos_islands,
	tuvalu: tuvalu,
	uganda: uganda,
	ukraine: ukraine,
	united_arab_emirates: united_arab_emirates,
	uk: uk,
	england: england,
	scotland: scotland,
	wales: wales,
	us: us,
	us_virgin_islands: us_virgin_islands,
	uruguay: uruguay,
	uzbekistan: uzbekistan,
	vanuatu: vanuatu,
	vatican_city: vatican_city,
	venezuela: venezuela,
	vietnam: vietnam,
	wallis_futuna: wallis_futuna,
	western_sahara: western_sahara,
	yemen: yemen,
	zambia: zambia,
	zimbabwe: zimbabwe,
	united_nations: united_nations,
	pirate_flag: pirate_flag
};

var require$$1 = [
	"grinning",
	"smiley",
	"smile",
	"grin",
	"laughing",
	"sweat_smile",
	"joy",
	"rofl",
	"relaxed",
	"blush",
	"innocent",
	"slightly_smiling_face",
	"upside_down_face",
	"wink",
	"relieved",
	"heart_eyes",
	"smiling_face_with_three_hearts",
	"kissing_heart",
	"kissing",
	"kissing_smiling_eyes",
	"kissing_closed_eyes",
	"yum",
	"stuck_out_tongue",
	"stuck_out_tongue_closed_eyes",
	"stuck_out_tongue_winking_eye",
	"zany",
	"raised_eyebrow",
	"monocle",
	"nerd_face",
	"sunglasses",
	"star_struck",
	"partying",
	"smirk",
	"unamused",
	"disappointed",
	"pensive",
	"worried",
	"confused",
	"slightly_frowning_face",
	"frowning_face",
	"persevere",
	"confounded",
	"tired_face",
	"weary",
	"pleading",
	"cry",
	"sob",
	"triumph",
	"angry",
	"rage",
	"symbols_over_mouth",
	"exploding_head",
	"flushed",
	"hot",
	"cold",
	"scream",
	"fearful",
	"cold_sweat",
	"disappointed_relieved",
	"sweat",
	"hugs",
	"thinking",
	"hand_over_mouth",
	"shushing",
	"lying_face",
	"no_mouth",
	"neutral_face",
	"expressionless",
	"grimacing",
	"roll_eyes",
	"hushed",
	"frowning",
	"anguished",
	"open_mouth",
	"astonished",
	"sleeping",
	"drooling_face",
	"sleepy",
	"dizzy_face",
	"zipper_mouth_face",
	"woozy",
	"nauseated_face",
	"vomiting",
	"sneezing_face",
	"mask",
	"face_with_thermometer",
	"face_with_head_bandage",
	"money_mouth_face",
	"cowboy_hat_face",
	"smiling_imp",
	"imp",
	"japanese_ogre",
	"japanese_goblin",
	"clown_face",
	"poop",
	"ghost",
	"skull",
	"skull_and_crossbones",
	"alien",
	"space_invader",
	"robot",
	"jack_o_lantern",
	"smiley_cat",
	"smile_cat",
	"joy_cat",
	"heart_eyes_cat",
	"smirk_cat",
	"kissing_cat",
	"scream_cat",
	"crying_cat_face",
	"pouting_cat",
	"palms_up",
	"open_hands",
	"raised_hands",
	"clap",
	"handshake",
	"+1",
	"-1",
	"facepunch",
	"fist",
	"fist_left",
	"fist_right",
	"crossed_fingers",
	"v",
	"love_you",
	"metal",
	"ok_hand",
	"point_left",
	"point_right",
	"point_up",
	"point_down",
	"point_up_2",
	"raised_hand",
	"raised_back_of_hand",
	"raised_hand_with_fingers_splayed",
	"vulcan_salute",
	"wave",
	"call_me_hand",
	"muscle",
	"fu",
	"writing_hand",
	"pray",
	"foot",
	"leg",
	"ring",
	"lipstick",
	"kiss",
	"lips",
	"tooth",
	"tongue",
	"ear",
	"nose",
	"footprints",
	"eye",
	"eyes",
	"brain",
	"speaking_head",
	"bust_in_silhouette",
	"busts_in_silhouette",
	"baby",
	"girl",
	"child",
	"boy",
	"woman",
	"adult",
	"man",
	"blonde_woman",
	"blonde_man",
	"bearded_person",
	"older_woman",
	"older_adult",
	"older_man",
	"man_with_gua_pi_mao",
	"woman_with_headscarf",
	"woman_with_turban",
	"man_with_turban",
	"policewoman",
	"policeman",
	"construction_worker_woman",
	"construction_worker_man",
	"guardswoman",
	"guardsman",
	"female_detective",
	"male_detective",
	"woman_health_worker",
	"man_health_worker",
	"woman_farmer",
	"man_farmer",
	"woman_cook",
	"man_cook",
	"woman_student",
	"man_student",
	"woman_singer",
	"man_singer",
	"woman_teacher",
	"man_teacher",
	"woman_factory_worker",
	"man_factory_worker",
	"woman_technologist",
	"man_technologist",
	"woman_office_worker",
	"man_office_worker",
	"woman_mechanic",
	"man_mechanic",
	"woman_scientist",
	"man_scientist",
	"woman_artist",
	"man_artist",
	"woman_firefighter",
	"man_firefighter",
	"woman_pilot",
	"man_pilot",
	"woman_astronaut",
	"man_astronaut",
	"woman_judge",
	"man_judge",
	"bride_with_veil",
	"man_in_tuxedo",
	"princess",
	"prince",
	"woman_superhero",
	"man_superhero",
	"woman_supervillain",
	"man_supervillain",
	"mrs_claus",
	"santa",
	"sorceress",
	"wizard",
	"woman_elf",
	"man_elf",
	"woman_vampire",
	"man_vampire",
	"woman_zombie",
	"man_zombie",
	"woman_genie",
	"man_genie",
	"mermaid",
	"merman",
	"woman_fairy",
	"man_fairy",
	"angel",
	"pregnant_woman",
	"breastfeeding",
	"bowing_woman",
	"bowing_man",
	"tipping_hand_woman",
	"tipping_hand_man",
	"no_good_woman",
	"no_good_man",
	"ok_woman",
	"ok_man",
	"raising_hand_woman",
	"raising_hand_man",
	"woman_facepalming",
	"man_facepalming",
	"woman_shrugging",
	"man_shrugging",
	"pouting_woman",
	"pouting_man",
	"frowning_woman",
	"frowning_man",
	"haircut_woman",
	"haircut_man",
	"massage_woman",
	"massage_man",
	"woman_in_steamy_room",
	"man_in_steamy_room",
	"nail_care",
	"selfie",
	"dancer",
	"man_dancing",
	"dancing_women",
	"dancing_men",
	"business_suit_levitating",
	"walking_woman",
	"walking_man",
	"running_woman",
	"running_man",
	"couple",
	"two_women_holding_hands",
	"two_men_holding_hands",
	"couple_with_heart_woman_man",
	"couple_with_heart_woman_woman",
	"couple_with_heart_man_man",
	"couplekiss_man_woman",
	"couplekiss_woman_woman",
	"couplekiss_man_man",
	"family_man_woman_boy",
	"family_man_woman_girl",
	"family_man_woman_girl_boy",
	"family_man_woman_boy_boy",
	"family_man_woman_girl_girl",
	"family_woman_woman_boy",
	"family_woman_woman_girl",
	"family_woman_woman_girl_boy",
	"family_woman_woman_boy_boy",
	"family_woman_woman_girl_girl",
	"family_man_man_boy",
	"family_man_man_girl",
	"family_man_man_girl_boy",
	"family_man_man_boy_boy",
	"family_man_man_girl_girl",
	"family_woman_boy",
	"family_woman_girl",
	"family_woman_girl_boy",
	"family_woman_boy_boy",
	"family_woman_girl_girl",
	"family_man_boy",
	"family_man_girl",
	"family_man_girl_boy",
	"family_man_boy_boy",
	"family_man_girl_girl",
	"yarn",
	"thread",
	"coat",
	"labcoat",
	"womans_clothes",
	"tshirt",
	"jeans",
	"necktie",
	"dress",
	"bikini",
	"kimono",
	"flat_shoe",
	"high_heel",
	"sandal",
	"boot",
	"mans_shoe",
	"athletic_shoe",
	"hiking_boot",
	"socks",
	"gloves",
	"scarf",
	"tophat",
	"billed_hat",
	"womans_hat",
	"mortar_board",
	"rescue_worker_helmet",
	"crown",
	"pouch",
	"purse",
	"handbag",
	"briefcase",
	"school_satchel",
	"luggage",
	"eyeglasses",
	"dark_sunglasses",
	"goggles",
	"closed_umbrella",
	"dog",
	"cat",
	"mouse",
	"hamster",
	"rabbit",
	"fox_face",
	"bear",
	"panda_face",
	"koala",
	"tiger",
	"lion",
	"cow",
	"pig",
	"pig_nose",
	"frog",
	"monkey_face",
	"see_no_evil",
	"hear_no_evil",
	"speak_no_evil",
	"monkey",
	"chicken",
	"penguin",
	"bird",
	"baby_chick",
	"hatching_chick",
	"hatched_chick",
	"duck",
	"eagle",
	"owl",
	"bat",
	"wolf",
	"boar",
	"horse",
	"unicorn",
	"honeybee",
	"bug",
	"butterfly",
	"snail",
	"shell",
	"beetle",
	"ant",
	"mosquito",
	"grasshopper",
	"spider",
	"spider_web",
	"scorpion",
	"turtle",
	"snake",
	"lizard",
	"t-rex",
	"sauropod",
	"octopus",
	"squid",
	"shrimp",
	"lobster",
	"crab",
	"blowfish",
	"tropical_fish",
	"fish",
	"dolphin",
	"whale",
	"whale2",
	"shark",
	"crocodile",
	"tiger2",
	"leopard",
	"zebra",
	"gorilla",
	"elephant",
	"hippopotamus",
	"rhinoceros",
	"dromedary_camel",
	"giraffe",
	"kangaroo",
	"camel",
	"water_buffalo",
	"ox",
	"cow2",
	"racehorse",
	"pig2",
	"ram",
	"sheep",
	"llama",
	"goat",
	"deer",
	"dog2",
	"poodle",
	"cat2",
	"rooster",
	"turkey",
	"peacock",
	"parrot",
	"swan",
	"dove",
	"rabbit2",
	"raccoon",
	"badger",
	"rat",
	"mouse2",
	"chipmunk",
	"hedgehog",
	"paw_prints",
	"dragon",
	"dragon_face",
	"cactus",
	"christmas_tree",
	"evergreen_tree",
	"deciduous_tree",
	"palm_tree",
	"seedling",
	"herb",
	"shamrock",
	"four_leaf_clover",
	"bamboo",
	"tanabata_tree",
	"leaves",
	"fallen_leaf",
	"maple_leaf",
	"ear_of_rice",
	"hibiscus",
	"sunflower",
	"rose",
	"wilted_flower",
	"tulip",
	"blossom",
	"cherry_blossom",
	"bouquet",
	"mushroom",
	"earth_americas",
	"earth_africa",
	"earth_asia",
	"full_moon",
	"waning_gibbous_moon",
	"last_quarter_moon",
	"waning_crescent_moon",
	"new_moon",
	"waxing_crescent_moon",
	"first_quarter_moon",
	"waxing_gibbous_moon",
	"new_moon_with_face",
	"full_moon_with_face",
	"first_quarter_moon_with_face",
	"last_quarter_moon_with_face",
	"sun_with_face",
	"crescent_moon",
	"star",
	"star2",
	"dizzy",
	"sparkles",
	"comet",
	"sunny",
	"sun_behind_small_cloud",
	"partly_sunny",
	"sun_behind_large_cloud",
	"sun_behind_rain_cloud",
	"cloud",
	"cloud_with_rain",
	"cloud_with_lightning_and_rain",
	"cloud_with_lightning",
	"zap",
	"fire",
	"boom",
	"snowflake",
	"cloud_with_snow",
	"snowman",
	"snowman_with_snow",
	"wind_face",
	"dash",
	"tornado",
	"fog",
	"open_umbrella",
	"umbrella",
	"droplet",
	"sweat_drops",
	"ocean",
	"green_apple",
	"apple",
	"pear",
	"tangerine",
	"lemon",
	"banana",
	"watermelon",
	"grapes",
	"strawberry",
	"melon",
	"cherries",
	"peach",
	"mango",
	"pineapple",
	"coconut",
	"kiwi_fruit",
	"tomato",
	"eggplant",
	"avocado",
	"broccoli",
	"leafy_greens",
	"cucumber",
	"hot_pepper",
	"corn",
	"carrot",
	"potato",
	"sweet_potato",
	"croissant",
	"bagel",
	"bread",
	"baguette_bread",
	"pretzel",
	"cheese",
	"egg",
	"fried_egg",
	"pancakes",
	"bacon",
	"steak",
	"poultry_leg",
	"meat_on_bone",
	"bone",
	"hotdog",
	"hamburger",
	"fries",
	"pizza",
	"sandwich",
	"stuffed_flatbread",
	"taco",
	"burrito",
	"green_salad",
	"shallow_pan_of_food",
	"canned_food",
	"spaghetti",
	"ramen",
	"stew",
	"curry",
	"sushi",
	"bento",
	"fried_shrimp",
	"rice_ball",
	"rice",
	"rice_cracker",
	"fish_cake",
	"fortune_cookie",
	"moon_cake",
	"oden",
	"dango",
	"shaved_ice",
	"ice_cream",
	"icecream",
	"pie",
	"cupcake",
	"cake",
	"birthday",
	"custard",
	"lollipop",
	"candy",
	"chocolate_bar",
	"popcorn",
	"doughnut",
	"dumpling",
	"cookie",
	"chestnut",
	"peanuts",
	"honey_pot",
	"milk_glass",
	"baby_bottle",
	"coffee",
	"tea",
	"cup_with_straw",
	"sake",
	"beer",
	"beers",
	"clinking_glasses",
	"wine_glass",
	"tumbler_glass",
	"cocktail",
	"tropical_drink",
	"champagne",
	"spoon",
	"fork_and_knife",
	"plate_with_cutlery",
	"bowl_with_spoon",
	"takeout_box",
	"chopsticks",
	"salt",
	"soccer",
	"basketball",
	"football",
	"baseball",
	"softball",
	"tennis",
	"volleyball",
	"rugby_football",
	"flying_disc",
	"8ball",
	"golf",
	"golfing_woman",
	"golfing_man",
	"ping_pong",
	"badminton",
	"goal_net",
	"ice_hockey",
	"field_hockey",
	"lacrosse",
	"cricket",
	"ski",
	"skier",
	"snowboarder",
	"person_fencing",
	"women_wrestling",
	"men_wrestling",
	"woman_cartwheeling",
	"man_cartwheeling",
	"woman_playing_handball",
	"man_playing_handball",
	"ice_skate",
	"curling_stone",
	"skateboard",
	"sled",
	"bow_and_arrow",
	"fishing_pole_and_fish",
	"boxing_glove",
	"martial_arts_uniform",
	"rowing_woman",
	"rowing_man",
	"climbing_woman",
	"climbing_man",
	"swimming_woman",
	"swimming_man",
	"woman_playing_water_polo",
	"man_playing_water_polo",
	"woman_in_lotus_position",
	"man_in_lotus_position",
	"surfing_woman",
	"surfing_man",
	"basketball_woman",
	"basketball_man",
	"weight_lifting_woman",
	"weight_lifting_man",
	"biking_woman",
	"biking_man",
	"mountain_biking_woman",
	"mountain_biking_man",
	"horse_racing",
	"trophy",
	"running_shirt_with_sash",
	"medal_sports",
	"medal_military",
	"1st_place_medal",
	"2nd_place_medal",
	"3rd_place_medal",
	"reminder_ribbon",
	"rosette",
	"ticket",
	"tickets",
	"performing_arts",
	"art",
	"circus_tent",
	"woman_juggling",
	"man_juggling",
	"microphone",
	"headphones",
	"musical_score",
	"musical_keyboard",
	"drum",
	"saxophone",
	"trumpet",
	"guitar",
	"violin",
	"clapper",
	"video_game",
	"dart",
	"game_die",
	"chess_pawn",
	"slot_machine",
	"jigsaw",
	"bowling",
	"red_car",
	"taxi",
	"blue_car",
	"bus",
	"trolleybus",
	"racing_car",
	"police_car",
	"ambulance",
	"fire_engine",
	"minibus",
	"truck",
	"articulated_lorry",
	"tractor",
	"kick_scooter",
	"motorcycle",
	"bike",
	"motor_scooter",
	"rotating_light",
	"oncoming_police_car",
	"oncoming_bus",
	"oncoming_automobile",
	"oncoming_taxi",
	"aerial_tramway",
	"mountain_cableway",
	"suspension_railway",
	"railway_car",
	"train",
	"monorail",
	"bullettrain_side",
	"bullettrain_front",
	"light_rail",
	"mountain_railway",
	"steam_locomotive",
	"train2",
	"metro",
	"tram",
	"station",
	"flying_saucer",
	"helicopter",
	"small_airplane",
	"airplane",
	"flight_departure",
	"flight_arrival",
	"sailboat",
	"motor_boat",
	"speedboat",
	"ferry",
	"passenger_ship",
	"rocket",
	"artificial_satellite",
	"seat",
	"canoe",
	"anchor",
	"construction",
	"fuelpump",
	"busstop",
	"vertical_traffic_light",
	"traffic_light",
	"ship",
	"ferris_wheel",
	"roller_coaster",
	"carousel_horse",
	"building_construction",
	"foggy",
	"tokyo_tower",
	"factory",
	"fountain",
	"rice_scene",
	"mountain",
	"mountain_snow",
	"mount_fuji",
	"volcano",
	"japan",
	"camping",
	"tent",
	"national_park",
	"motorway",
	"railway_track",
	"sunrise",
	"sunrise_over_mountains",
	"desert",
	"beach_umbrella",
	"desert_island",
	"city_sunrise",
	"city_sunset",
	"cityscape",
	"night_with_stars",
	"bridge_at_night",
	"milky_way",
	"stars",
	"sparkler",
	"fireworks",
	"rainbow",
	"houses",
	"european_castle",
	"japanese_castle",
	"stadium",
	"statue_of_liberty",
	"house",
	"house_with_garden",
	"derelict_house",
	"office",
	"department_store",
	"post_office",
	"european_post_office",
	"hospital",
	"bank",
	"hotel",
	"convenience_store",
	"school",
	"love_hotel",
	"wedding",
	"classical_building",
	"church",
	"mosque",
	"synagogue",
	"kaaba",
	"shinto_shrine",
	"watch",
	"iphone",
	"calling",
	"computer",
	"keyboard",
	"desktop_computer",
	"printer",
	"computer_mouse",
	"trackball",
	"joystick",
	"clamp",
	"minidisc",
	"floppy_disk",
	"cd",
	"dvd",
	"vhs",
	"camera",
	"camera_flash",
	"video_camera",
	"movie_camera",
	"film_projector",
	"film_strip",
	"telephone_receiver",
	"phone",
	"pager",
	"fax",
	"tv",
	"radio",
	"studio_microphone",
	"level_slider",
	"control_knobs",
	"compass",
	"stopwatch",
	"timer_clock",
	"alarm_clock",
	"mantelpiece_clock",
	"hourglass_flowing_sand",
	"hourglass",
	"satellite",
	"battery",
	"electric_plug",
	"bulb",
	"flashlight",
	"candle",
	"fire_extinguisher",
	"wastebasket",
	"oil_drum",
	"money_with_wings",
	"dollar",
	"yen",
	"euro",
	"pound",
	"moneybag",
	"credit_card",
	"gem",
	"balance_scale",
	"toolbox",
	"wrench",
	"hammer",
	"hammer_and_pick",
	"hammer_and_wrench",
	"pick",
	"nut_and_bolt",
	"gear",
	"brick",
	"chains",
	"magnet",
	"gun",
	"bomb",
	"firecracker",
	"hocho",
	"dagger",
	"crossed_swords",
	"shield",
	"smoking",
	"coffin",
	"funeral_urn",
	"amphora",
	"crystal_ball",
	"prayer_beads",
	"nazar_amulet",
	"barber",
	"alembic",
	"telescope",
	"microscope",
	"hole",
	"pill",
	"syringe",
	"dna",
	"microbe",
	"petri_dish",
	"test_tube",
	"thermometer",
	"broom",
	"basket",
	"toilet_paper",
	"label",
	"bookmark",
	"toilet",
	"shower",
	"bathtub",
	"bath",
	"soap",
	"sponge",
	"lotion_bottle",
	"key",
	"old_key",
	"couch_and_lamp",
	"sleeping_bed",
	"bed",
	"door",
	"bellhop_bell",
	"teddy_bear",
	"framed_picture",
	"world_map",
	"parasol_on_ground",
	"moyai",
	"shopping",
	"shopping_cart",
	"balloon",
	"flags",
	"ribbon",
	"gift",
	"confetti_ball",
	"tada",
	"dolls",
	"wind_chime",
	"crossed_flags",
	"izakaya_lantern",
	"red_envelope",
	"email",
	"envelope_with_arrow",
	"incoming_envelope",
	"e-mail",
	"love_letter",
	"postbox",
	"mailbox_closed",
	"mailbox",
	"mailbox_with_mail",
	"mailbox_with_no_mail",
	"package",
	"postal_horn",
	"inbox_tray",
	"outbox_tray",
	"scroll",
	"page_with_curl",
	"bookmark_tabs",
	"receipt",
	"bar_chart",
	"chart_with_upwards_trend",
	"chart_with_downwards_trend",
	"page_facing_up",
	"date",
	"calendar",
	"spiral_calendar",
	"card_index",
	"card_file_box",
	"ballot_box",
	"file_cabinet",
	"clipboard",
	"spiral_notepad",
	"file_folder",
	"open_file_folder",
	"card_index_dividers",
	"newspaper_roll",
	"newspaper",
	"notebook",
	"closed_book",
	"green_book",
	"blue_book",
	"orange_book",
	"notebook_with_decorative_cover",
	"ledger",
	"books",
	"open_book",
	"safety_pin",
	"link",
	"paperclip",
	"paperclips",
	"scissors",
	"triangular_ruler",
	"straight_ruler",
	"abacus",
	"pushpin",
	"round_pushpin",
	"closed_lock_with_key",
	"lock",
	"unlock",
	"lock_with_ink_pen",
	"pen",
	"fountain_pen",
	"black_nib",
	"memo",
	"pencil2",
	"crayon",
	"paintbrush",
	"mag",
	"mag_right",
	"heart",
	"orange_heart",
	"yellow_heart",
	"green_heart",
	"blue_heart",
	"purple_heart",
	"black_heart",
	"broken_heart",
	"heavy_heart_exclamation",
	"two_hearts",
	"revolving_hearts",
	"heartbeat",
	"heartpulse",
	"sparkling_heart",
	"cupid",
	"gift_heart",
	"heart_decoration",
	"peace_symbol",
	"latin_cross",
	"star_and_crescent",
	"om",
	"wheel_of_dharma",
	"star_of_david",
	"six_pointed_star",
	"menorah",
	"yin_yang",
	"orthodox_cross",
	"place_of_worship",
	"ophiuchus",
	"aries",
	"taurus",
	"gemini",
	"cancer",
	"leo",
	"virgo",
	"libra",
	"scorpius",
	"sagittarius",
	"capricorn",
	"aquarius",
	"pisces",
	"id",
	"atom_symbol",
	"u7a7a",
	"u5272",
	"radioactive",
	"biohazard",
	"mobile_phone_off",
	"vibration_mode",
	"u6709",
	"u7121",
	"u7533",
	"u55b6",
	"u6708",
	"eight_pointed_black_star",
	"vs",
	"accept",
	"white_flower",
	"ideograph_advantage",
	"secret",
	"congratulations",
	"u5408",
	"u6e80",
	"u7981",
	"a",
	"b",
	"ab",
	"cl",
	"o2",
	"sos",
	"no_entry",
	"name_badge",
	"no_entry_sign",
	"x",
	"o",
	"stop_sign",
	"anger",
	"hotsprings",
	"no_pedestrians",
	"do_not_litter",
	"no_bicycles",
	"non-potable_water",
	"underage",
	"no_mobile_phones",
	"exclamation",
	"grey_exclamation",
	"question",
	"grey_question",
	"bangbang",
	"interrobang",
	"100",
	"low_brightness",
	"high_brightness",
	"trident",
	"fleur_de_lis",
	"part_alternation_mark",
	"warning",
	"children_crossing",
	"beginner",
	"recycle",
	"u6307",
	"chart",
	"sparkle",
	"eight_spoked_asterisk",
	"negative_squared_cross_mark",
	"white_check_mark",
	"diamond_shape_with_a_dot_inside",
	"cyclone",
	"loop",
	"globe_with_meridians",
	"m",
	"atm",
	"zzz",
	"sa",
	"passport_control",
	"customs",
	"baggage_claim",
	"left_luggage",
	"wheelchair",
	"no_smoking",
	"wc",
	"parking",
	"potable_water",
	"mens",
	"womens",
	"baby_symbol",
	"restroom",
	"put_litter_in_its_place",
	"cinema",
	"signal_strength",
	"koko",
	"ng",
	"ok",
	"up",
	"cool",
	"new",
	"free",
	"zero",
	"one",
	"two",
	"three",
	"four",
	"five",
	"six",
	"seven",
	"eight",
	"nine",
	"keycap_ten",
	"asterisk",
	"1234",
	"eject_button",
	"arrow_forward",
	"pause_button",
	"next_track_button",
	"stop_button",
	"record_button",
	"play_or_pause_button",
	"previous_track_button",
	"fast_forward",
	"rewind",
	"twisted_rightwards_arrows",
	"repeat",
	"repeat_one",
	"arrow_backward",
	"arrow_up_small",
	"arrow_down_small",
	"arrow_double_up",
	"arrow_double_down",
	"arrow_right",
	"arrow_left",
	"arrow_up",
	"arrow_down",
	"arrow_upper_right",
	"arrow_lower_right",
	"arrow_lower_left",
	"arrow_upper_left",
	"arrow_up_down",
	"left_right_arrow",
	"arrows_counterclockwise",
	"arrow_right_hook",
	"leftwards_arrow_with_hook",
	"arrow_heading_up",
	"arrow_heading_down",
	"hash",
	"information_source",
	"abc",
	"abcd",
	"capital_abcd",
	"symbols",
	"musical_note",
	"notes",
	"wavy_dash",
	"curly_loop",
	"heavy_check_mark",
	"arrows_clockwise",
	"heavy_plus_sign",
	"heavy_minus_sign",
	"heavy_division_sign",
	"heavy_multiplication_x",
	"infinity",
	"heavy_dollar_sign",
	"currency_exchange",
	"copyright",
	"registered",
	"tm",
	"end",
	"back",
	"on",
	"top",
	"soon",
	"ballot_box_with_check",
	"radio_button",
	"white_circle",
	"black_circle",
	"red_circle",
	"large_blue_circle",
	"small_orange_diamond",
	"small_blue_diamond",
	"large_orange_diamond",
	"large_blue_diamond",
	"small_red_triangle",
	"black_small_square",
	"white_small_square",
	"black_large_square",
	"white_large_square",
	"small_red_triangle_down",
	"black_medium_square",
	"white_medium_square",
	"black_medium_small_square",
	"white_medium_small_square",
	"black_square_button",
	"white_square_button",
	"speaker",
	"sound",
	"loud_sound",
	"mute",
	"mega",
	"loudspeaker",
	"bell",
	"no_bell",
	"black_joker",
	"mahjong",
	"spades",
	"clubs",
	"hearts",
	"diamonds",
	"flower_playing_cards",
	"thought_balloon",
	"right_anger_bubble",
	"speech_balloon",
	"left_speech_bubble",
	"clock1",
	"clock2",
	"clock3",
	"clock4",
	"clock5",
	"clock6",
	"clock7",
	"clock8",
	"clock9",
	"clock10",
	"clock11",
	"clock12",
	"clock130",
	"clock230",
	"clock330",
	"clock430",
	"clock530",
	"clock630",
	"clock730",
	"clock830",
	"clock930",
	"clock1030",
	"clock1130",
	"clock1230",
	"white_flag",
	"black_flag",
	"pirate_flag",
	"checkered_flag",
	"triangular_flag_on_post",
	"rainbow_flag",
	"united_nations",
	"afghanistan",
	"aland_islands",
	"albania",
	"algeria",
	"american_samoa",
	"andorra",
	"angola",
	"anguilla",
	"antarctica",
	"antigua_barbuda",
	"argentina",
	"armenia",
	"aruba",
	"australia",
	"austria",
	"azerbaijan",
	"bahamas",
	"bahrain",
	"bangladesh",
	"barbados",
	"belarus",
	"belgium",
	"belize",
	"benin",
	"bermuda",
	"bhutan",
	"bolivia",
	"caribbean_netherlands",
	"bosnia_herzegovina",
	"botswana",
	"brazil",
	"british_indian_ocean_territory",
	"british_virgin_islands",
	"brunei",
	"bulgaria",
	"burkina_faso",
	"burundi",
	"cape_verde",
	"cambodia",
	"cameroon",
	"canada",
	"canary_islands",
	"cayman_islands",
	"central_african_republic",
	"chad",
	"chile",
	"cn",
	"christmas_island",
	"cocos_islands",
	"colombia",
	"comoros",
	"congo_brazzaville",
	"congo_kinshasa",
	"cook_islands",
	"costa_rica",
	"croatia",
	"cuba",
	"curacao",
	"cyprus",
	"czech_republic",
	"denmark",
	"djibouti",
	"dominica",
	"dominican_republic",
	"ecuador",
	"egypt",
	"el_salvador",
	"equatorial_guinea",
	"eritrea",
	"estonia",
	"ethiopia",
	"eu",
	"falkland_islands",
	"faroe_islands",
	"fiji",
	"finland",
	"fr",
	"french_guiana",
	"french_polynesia",
	"french_southern_territories",
	"gabon",
	"gambia",
	"georgia",
	"de",
	"ghana",
	"gibraltar",
	"greece",
	"greenland",
	"grenada",
	"guadeloupe",
	"guam",
	"guatemala",
	"guernsey",
	"guinea",
	"guinea_bissau",
	"guyana",
	"haiti",
	"honduras",
	"hong_kong",
	"hungary",
	"iceland",
	"india",
	"indonesia",
	"iran",
	"iraq",
	"ireland",
	"isle_of_man",
	"israel",
	"it",
	"cote_divoire",
	"jamaica",
	"jp",
	"jersey",
	"jordan",
	"kazakhstan",
	"kenya",
	"kiribati",
	"kosovo",
	"kuwait",
	"kyrgyzstan",
	"laos",
	"latvia",
	"lebanon",
	"lesotho",
	"liberia",
	"libya",
	"liechtenstein",
	"lithuania",
	"luxembourg",
	"macau",
	"macedonia",
	"madagascar",
	"malawi",
	"malaysia",
	"maldives",
	"mali",
	"malta",
	"marshall_islands",
	"martinique",
	"mauritania",
	"mauritius",
	"mayotte",
	"mexico",
	"micronesia",
	"moldova",
	"monaco",
	"mongolia",
	"montenegro",
	"montserrat",
	"morocco",
	"mozambique",
	"myanmar",
	"namibia",
	"nauru",
	"nepal",
	"netherlands",
	"new_caledonia",
	"new_zealand",
	"nicaragua",
	"niger",
	"nigeria",
	"niue",
	"norfolk_island",
	"northern_mariana_islands",
	"north_korea",
	"norway",
	"oman",
	"pakistan",
	"palau",
	"palestinian_territories",
	"panama",
	"papua_new_guinea",
	"paraguay",
	"peru",
	"philippines",
	"pitcairn_islands",
	"poland",
	"portugal",
	"puerto_rico",
	"qatar",
	"reunion",
	"romania",
	"ru",
	"rwanda",
	"st_barthelemy",
	"st_helena",
	"st_kitts_nevis",
	"st_lucia",
	"st_pierre_miquelon",
	"st_vincent_grenadines",
	"samoa",
	"san_marino",
	"sao_tome_principe",
	"saudi_arabia",
	"senegal",
	"serbia",
	"seychelles",
	"sierra_leone",
	"singapore",
	"sint_maarten",
	"slovakia",
	"slovenia",
	"solomon_islands",
	"somalia",
	"south_africa",
	"south_georgia_south_sandwich_islands",
	"kr",
	"south_sudan",
	"es",
	"sri_lanka",
	"sudan",
	"suriname",
	"swaziland",
	"sweden",
	"switzerland",
	"syria",
	"taiwan",
	"tajikistan",
	"tanzania",
	"thailand",
	"timor_leste",
	"togo",
	"tokelau",
	"tonga",
	"trinidad_tobago",
	"tunisia",
	"tr",
	"turkmenistan",
	"turks_caicos_islands",
	"tuvalu",
	"uganda",
	"ukraine",
	"united_arab_emirates",
	"uk",
	"england",
	"scotland",
	"wales",
	"us",
	"us_virgin_islands",
	"uruguay",
	"uzbekistan",
	"vanuatu",
	"vatican_city",
	"venezuela",
	"vietnam",
	"wallis_futuna",
	"western_sahara",
	"yemen",
	"zambia",
	"zimbabwe"
];

var emojilib = {
  lib: require$$0,
  ordered: require$$1,
  fitzpatrick_scale_modifiers: ["🏻", "🏼", "🏽", "🏾", "🏿"]
};

var indicatorStyle = 'color: var(--text-accent); width: 2.5em; text-align: center; float:left; font-weight:800;';
var MyPlugin = /** @class */ (function (_super) {
    __extends(MyPlugin, _super);
    function MyPlugin() {
        return _super !== null && _super.apply(this, arguments) || this;
    }
    MyPlugin.prototype.loadEmojis = function () {
        function titleCase(string) {
            var sentence = string.toLowerCase().split('_');
            for (var i = 0; i < sentence.length; i++) {
                sentence[i] = sentence[i][0].toUpperCase() + sentence[i].slice(1);
            }
            return sentence.join(' ');
        }
        var items = emojilib.ordered.map(function (name) {
            return {
                name: titleCase(name),
                char: emojilib.lib[name].char,
            };
        });
        return items;
    };
    MyPlugin.prototype.onload = function () {
        var _this = this;
        this.emojis = this.loadEmojis();
        this.addCommand({
            id: 'emoji-picker:open-picker',
            name: 'Open emoji picker',
            hotkeys: [],
            checkCallback: function (checking) {
                var leaf = _this.app.workspace.activeLeaf;
                if (leaf) {
                    if (!checking) {
                        new EmojiFuzzySuggestModal(_this.app, _this.emojis).open();
                    }
                    return true;
                }
                return false;
            }
        });
    };
    MyPlugin.prototype.onunload = function () {
    };
    return MyPlugin;
}(obsidian.Plugin));
var EmojiFuzzySuggestModal = /** @class */ (function (_super) {
    __extends(EmojiFuzzySuggestModal, _super);
    function EmojiFuzzySuggestModal(app, emojis) {
        var _this = _super.call(this, app) || this;
        _this.app = app;
        _this.emojis = emojis;
        return _this;
    }
    EmojiFuzzySuggestModal.prototype.getItems = function () {
        return this.emojis;
    };
    EmojiFuzzySuggestModal.prototype.getItemText = function (item) {
        return item.name;
    };
    EmojiFuzzySuggestModal.prototype.renderSuggestion = function (item, el) {
        _super.prototype.renderSuggestion.call(this, item, el);
        this.updateSuggestionElForMode(item, el);
    };
    EmojiFuzzySuggestModal.prototype.updateSuggestionElForMode = function (item, el) {
        var indicatorEl = createEl('div', {
            text: item.item.char,
            attr: { style: indicatorStyle },
        });
        el.insertAdjacentElement('afterbegin', indicatorEl);
    };
    EmojiFuzzySuggestModal.prototype.insertTextAtCursor = function (view, text) {
        var editor = view.sourceMode.cmEditor;
        var doc = editor.getDoc();
        var cursor = doc.getCursor();
        doc.replaceRange(text, cursor);
    };
    EmojiFuzzySuggestModal.prototype.onChooseItem = function (item, evt) {
        var activeEditor = this.app.workspace.getActiveViewOfType(obsidian.MarkdownView);
        if (activeEditor) {
            this.insertTextAtCursor(activeEditor, item.char);
        }
        else {
            new obsidian.Notice("You'll need to open a markdown editor to insert an emoji");
        }
    };
    return EmojiFuzzySuggestModal;
}(obsidian.FuzzySuggestModal));

module.exports = MyPlugin;
//# sourceMappingURL=data:application/json;charset=utf-8;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoibWFpbi5qcyIsInNvdXJjZXMiOlsibm9kZV9tb2R1bGVzL3RzbGliL3RzbGliLmVzNi5qcyIsIm5vZGVfbW9kdWxlcy9lbW9qaWxpYi9pbmRleC5qcyIsIm1haW4udHMiXSwic291cmNlc0NvbnRlbnQiOlsiLyohICoqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqXHJcbkNvcHlyaWdodCAoYykgTWljcm9zb2Z0IENvcnBvcmF0aW9uLlxyXG5cclxuUGVybWlzc2lvbiB0byB1c2UsIGNvcHksIG1vZGlmeSwgYW5kL29yIGRpc3RyaWJ1dGUgdGhpcyBzb2Z0d2FyZSBmb3IgYW55XHJcbnB1cnBvc2Ugd2l0aCBvciB3aXRob3V0IGZlZSBpcyBoZXJlYnkgZ3JhbnRlZC5cclxuXHJcblRIRSBTT0ZUV0FSRSBJUyBQUk9WSURFRCBcIkFTIElTXCIgQU5EIFRIRSBBVVRIT1IgRElTQ0xBSU1TIEFMTCBXQVJSQU5USUVTIFdJVEhcclxuUkVHQVJEIFRPIFRISVMgU09GVFdBUkUgSU5DTFVESU5HIEFMTCBJTVBMSUVEIFdBUlJBTlRJRVMgT0YgTUVSQ0hBTlRBQklMSVRZXHJcbkFORCBGSVRORVNTLiBJTiBOTyBFVkVOVCBTSEFMTCBUSEUgQVVUSE9SIEJFIExJQUJMRSBGT1IgQU5ZIFNQRUNJQUwsIERJUkVDVCxcclxuSU5ESVJFQ1QsIE9SIENPTlNFUVVFTlRJQUwgREFNQUdFUyBPUiBBTlkgREFNQUdFUyBXSEFUU09FVkVSIFJFU1VMVElORyBGUk9NXHJcbkxPU1MgT0YgVVNFLCBEQVRBIE9SIFBST0ZJVFMsIFdIRVRIRVIgSU4gQU4gQUNUSU9OIE9GIENPTlRSQUNULCBORUdMSUdFTkNFIE9SXHJcbk9USEVSIFRPUlRJT1VTIEFDVElPTiwgQVJJU0lORyBPVVQgT0YgT1IgSU4gQ09OTkVDVElPTiBXSVRIIFRIRSBVU0UgT1JcclxuUEVSRk9STUFOQ0UgT0YgVEhJUyBTT0ZUV0FSRS5cclxuKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKiogKi9cclxuLyogZ2xvYmFsIFJlZmxlY3QsIFByb21pc2UgKi9cclxuXHJcbnZhciBleHRlbmRTdGF0aWNzID0gZnVuY3Rpb24oZCwgYikge1xyXG4gICAgZXh0ZW5kU3RhdGljcyA9IE9iamVjdC5zZXRQcm90b3R5cGVPZiB8fFxyXG4gICAgICAgICh7IF9fcHJvdG9fXzogW10gfSBpbnN0YW5jZW9mIEFycmF5ICYmIGZ1bmN0aW9uIChkLCBiKSB7IGQuX19wcm90b19fID0gYjsgfSkgfHxcclxuICAgICAgICBmdW5jdGlvbiAoZCwgYikgeyBmb3IgKHZhciBwIGluIGIpIGlmIChPYmplY3QucHJvdG90eXBlLmhhc093blByb3BlcnR5LmNhbGwoYiwgcCkpIGRbcF0gPSBiW3BdOyB9O1xyXG4gICAgcmV0dXJuIGV4dGVuZFN0YXRpY3MoZCwgYik7XHJcbn07XHJcblxyXG5leHBvcnQgZnVuY3Rpb24gX19leHRlbmRzKGQsIGIpIHtcclxuICAgIGV4dGVuZFN0YXRpY3MoZCwgYik7XHJcbiAgICBmdW5jdGlvbiBfXygpIHsgdGhpcy5jb25zdHJ1Y3RvciA9IGQ7IH1cclxuICAgIGQucHJvdG90eXBlID0gYiA9PT0gbnVsbCA/IE9iamVjdC5jcmVhdGUoYikgOiAoX18ucHJvdG90eXBlID0gYi5wcm90b3R5cGUsIG5ldyBfXygpKTtcclxufVxyXG5cclxuZXhwb3J0IHZhciBfX2Fzc2lnbiA9IGZ1bmN0aW9uKCkge1xyXG4gICAgX19hc3NpZ24gPSBPYmplY3QuYXNzaWduIHx8IGZ1bmN0aW9uIF9fYXNzaWduKHQpIHtcclxuICAgICAgICBmb3IgKHZhciBzLCBpID0gMSwgbiA9IGFyZ3VtZW50cy5sZW5ndGg7IGkgPCBuOyBpKyspIHtcclxuICAgICAgICAgICAgcyA9IGFyZ3VtZW50c1tpXTtcclxuICAgICAgICAgICAgZm9yICh2YXIgcCBpbiBzKSBpZiAoT2JqZWN0LnByb3RvdHlwZS5oYXNPd25Qcm9wZXJ0eS5jYWxsKHMsIHApKSB0W3BdID0gc1twXTtcclxuICAgICAgICB9XHJcbiAgICAgICAgcmV0dXJuIHQ7XHJcbiAgICB9XHJcbiAgICByZXR1cm4gX19hc3NpZ24uYXBwbHkodGhpcywgYXJndW1lbnRzKTtcclxufVxyXG5cclxuZXhwb3J0IGZ1bmN0aW9uIF9fcmVzdChzLCBlKSB7XHJcbiAgICB2YXIgdCA9IHt9O1xyXG4gICAgZm9yICh2YXIgcCBpbiBzKSBpZiAoT2JqZWN0LnByb3RvdHlwZS5oYXNPd25Qcm9wZXJ0eS5jYWxsKHMsIHApICYmIGUuaW5kZXhPZihwKSA8IDApXHJcbiAgICAgICAgdFtwXSA9IHNbcF07XHJcbiAgICBpZiAocyAhPSBudWxsICYmIHR5cGVvZiBPYmplY3QuZ2V0T3duUHJvcGVydHlTeW1ib2xzID09PSBcImZ1bmN0aW9uXCIpXHJcbiAgICAgICAgZm9yICh2YXIgaSA9IDAsIHAgPSBPYmplY3QuZ2V0T3duUHJvcGVydHlTeW1ib2xzKHMpOyBpIDwgcC5sZW5ndGg7IGkrKykge1xyXG4gICAgICAgICAgICBpZiAoZS5pbmRleE9mKHBbaV0pIDwgMCAmJiBPYmplY3QucHJvdG90eXBlLnByb3BlcnR5SXNFbnVtZXJhYmxlLmNhbGwocywgcFtpXSkpXHJcbiAgICAgICAgICAgICAgICB0W3BbaV1dID0gc1twW2ldXTtcclxuICAgICAgICB9XHJcbiAgICByZXR1cm4gdDtcclxufVxyXG5cclxuZXhwb3J0IGZ1bmN0aW9uIF9fZGVjb3JhdGUoZGVjb3JhdG9ycywgdGFyZ2V0LCBrZXksIGRlc2MpIHtcclxuICAgIHZhciBjID0gYXJndW1lbnRzLmxlbmd0aCwgciA9IGMgPCAzID8gdGFyZ2V0IDogZGVzYyA9PT0gbnVsbCA/IGRlc2MgPSBPYmplY3QuZ2V0T3duUHJvcGVydHlEZXNjcmlwdG9yKHRhcmdldCwga2V5KSA6IGRlc2MsIGQ7XHJcbiAgICBpZiAodHlwZW9mIFJlZmxlY3QgPT09IFwib2JqZWN0XCIgJiYgdHlwZW9mIFJlZmxlY3QuZGVjb3JhdGUgPT09IFwiZnVuY3Rpb25cIikgciA9IFJlZmxlY3QuZGVjb3JhdGUoZGVjb3JhdG9ycywgdGFyZ2V0LCBrZXksIGRlc2MpO1xyXG4gICAgZWxzZSBmb3IgKHZhciBpID0gZGVjb3JhdG9ycy5sZW5ndGggLSAxOyBpID49IDA7IGktLSkgaWYgKGQgPSBkZWNvcmF0b3JzW2ldKSByID0gKGMgPCAzID8gZChyKSA6IGMgPiAzID8gZCh0YXJnZXQsIGtleSwgcikgOiBkKHRhcmdldCwga2V5KSkgfHwgcjtcclxuICAgIHJldHVybiBjID4gMyAmJiByICYmIE9iamVjdC5kZWZpbmVQcm9wZXJ0eSh0YXJnZXQsIGtleSwgciksIHI7XHJcbn1cclxuXHJcbmV4cG9ydCBmdW5jdGlvbiBfX3BhcmFtKHBhcmFtSW5kZXgsIGRlY29yYXRvcikge1xyXG4gICAgcmV0dXJuIGZ1bmN0aW9uICh0YXJnZXQsIGtleSkgeyBkZWNvcmF0b3IodGFyZ2V0LCBrZXksIHBhcmFtSW5kZXgpOyB9XHJcbn1cclxuXHJcbmV4cG9ydCBmdW5jdGlvbiBfX21ldGFkYXRhKG1ldGFkYXRhS2V5LCBtZXRhZGF0YVZhbHVlKSB7XHJcbiAgICBpZiAodHlwZW9mIFJlZmxlY3QgPT09IFwib2JqZWN0XCIgJiYgdHlwZW9mIFJlZmxlY3QubWV0YWRhdGEgPT09IFwiZnVuY3Rpb25cIikgcmV0dXJuIFJlZmxlY3QubWV0YWRhdGEobWV0YWRhdGFLZXksIG1ldGFkYXRhVmFsdWUpO1xyXG59XHJcblxyXG5leHBvcnQgZnVuY3Rpb24gX19hd2FpdGVyKHRoaXNBcmcsIF9hcmd1bWVudHMsIFAsIGdlbmVyYXRvcikge1xyXG4gICAgZnVuY3Rpb24gYWRvcHQodmFsdWUpIHsgcmV0dXJuIHZhbHVlIGluc3RhbmNlb2YgUCA/IHZhbHVlIDogbmV3IFAoZnVuY3Rpb24gKHJlc29sdmUpIHsgcmVzb2x2ZSh2YWx1ZSk7IH0pOyB9XHJcbiAgICByZXR1cm4gbmV3IChQIHx8IChQID0gUHJvbWlzZSkpKGZ1bmN0aW9uIChyZXNvbHZlLCByZWplY3QpIHtcclxuICAgICAgICBmdW5jdGlvbiBmdWxmaWxsZWQodmFsdWUpIHsgdHJ5IHsgc3RlcChnZW5lcmF0b3IubmV4dCh2YWx1ZSkpOyB9IGNhdGNoIChlKSB7IHJlamVjdChlKTsgfSB9XHJcbiAgICAgICAgZnVuY3Rpb24gcmVqZWN0ZWQodmFsdWUpIHsgdHJ5IHsgc3RlcChnZW5lcmF0b3JbXCJ0aHJvd1wiXSh2YWx1ZSkpOyB9IGNhdGNoIChlKSB7IHJlamVjdChlKTsgfSB9XHJcbiAgICAgICAgZnVuY3Rpb24gc3RlcChyZXN1bHQpIHsgcmVzdWx0LmRvbmUgPyByZXNvbHZlKHJlc3VsdC52YWx1ZSkgOiBhZG9wdChyZXN1bHQudmFsdWUpLnRoZW4oZnVsZmlsbGVkLCByZWplY3RlZCk7IH1cclxuICAgICAgICBzdGVwKChnZW5lcmF0b3IgPSBnZW5lcmF0b3IuYXBwbHkodGhpc0FyZywgX2FyZ3VtZW50cyB8fCBbXSkpLm5leHQoKSk7XHJcbiAgICB9KTtcclxufVxyXG5cclxuZXhwb3J0IGZ1bmN0aW9uIF9fZ2VuZXJhdG9yKHRoaXNBcmcsIGJvZHkpIHtcclxuICAgIHZhciBfID0geyBsYWJlbDogMCwgc2VudDogZnVuY3Rpb24oKSB7IGlmICh0WzBdICYgMSkgdGhyb3cgdFsxXTsgcmV0dXJuIHRbMV07IH0sIHRyeXM6IFtdLCBvcHM6IFtdIH0sIGYsIHksIHQsIGc7XHJcbiAgICByZXR1cm4gZyA9IHsgbmV4dDogdmVyYigwKSwgXCJ0aHJvd1wiOiB2ZXJiKDEpLCBcInJldHVyblwiOiB2ZXJiKDIpIH0sIHR5cGVvZiBTeW1ib2wgPT09IFwiZnVuY3Rpb25cIiAmJiAoZ1tTeW1ib2wuaXRlcmF0b3JdID0gZnVuY3Rpb24oKSB7IHJldHVybiB0aGlzOyB9KSwgZztcclxuICAgIGZ1bmN0aW9uIHZlcmIobikgeyByZXR1cm4gZnVuY3Rpb24gKHYpIHsgcmV0dXJuIHN0ZXAoW24sIHZdKTsgfTsgfVxyXG4gICAgZnVuY3Rpb24gc3RlcChvcCkge1xyXG4gICAgICAgIGlmIChmKSB0aHJvdyBuZXcgVHlwZUVycm9yKFwiR2VuZXJhdG9yIGlzIGFscmVhZHkgZXhlY3V0aW5nLlwiKTtcclxuICAgICAgICB3aGlsZSAoXykgdHJ5IHtcclxuICAgICAgICAgICAgaWYgKGYgPSAxLCB5ICYmICh0ID0gb3BbMF0gJiAyID8geVtcInJldHVyblwiXSA6IG9wWzBdID8geVtcInRocm93XCJdIHx8ICgodCA9IHlbXCJyZXR1cm5cIl0pICYmIHQuY2FsbCh5KSwgMCkgOiB5Lm5leHQpICYmICEodCA9IHQuY2FsbCh5LCBvcFsxXSkpLmRvbmUpIHJldHVybiB0O1xyXG4gICAgICAgICAgICBpZiAoeSA9IDAsIHQpIG9wID0gW29wWzBdICYgMiwgdC52YWx1ZV07XHJcbiAgICAgICAgICAgIHN3aXRjaCAob3BbMF0pIHtcclxuICAgICAgICAgICAgICAgIGNhc2UgMDogY2FzZSAxOiB0ID0gb3A7IGJyZWFrO1xyXG4gICAgICAgICAgICAgICAgY2FzZSA0OiBfLmxhYmVsKys7IHJldHVybiB7IHZhbHVlOiBvcFsxXSwgZG9uZTogZmFsc2UgfTtcclxuICAgICAgICAgICAgICAgIGNhc2UgNTogXy5sYWJlbCsrOyB5ID0gb3BbMV07IG9wID0gWzBdOyBjb250aW51ZTtcclxuICAgICAgICAgICAgICAgIGNhc2UgNzogb3AgPSBfLm9wcy5wb3AoKTsgXy50cnlzLnBvcCgpOyBjb250aW51ZTtcclxuICAgICAgICAgICAgICAgIGRlZmF1bHQ6XHJcbiAgICAgICAgICAgICAgICAgICAgaWYgKCEodCA9IF8udHJ5cywgdCA9IHQubGVuZ3RoID4gMCAmJiB0W3QubGVuZ3RoIC0gMV0pICYmIChvcFswXSA9PT0gNiB8fCBvcFswXSA9PT0gMikpIHsgXyA9IDA7IGNvbnRpbnVlOyB9XHJcbiAgICAgICAgICAgICAgICAgICAgaWYgKG9wWzBdID09PSAzICYmICghdCB8fCAob3BbMV0gPiB0WzBdICYmIG9wWzFdIDwgdFszXSkpKSB7IF8ubGFiZWwgPSBvcFsxXTsgYnJlYWs7IH1cclxuICAgICAgICAgICAgICAgICAgICBpZiAob3BbMF0gPT09IDYgJiYgXy5sYWJlbCA8IHRbMV0pIHsgXy5sYWJlbCA9IHRbMV07IHQgPSBvcDsgYnJlYWs7IH1cclxuICAgICAgICAgICAgICAgICAgICBpZiAodCAmJiBfLmxhYmVsIDwgdFsyXSkgeyBfLmxhYmVsID0gdFsyXTsgXy5vcHMucHVzaChvcCk7IGJyZWFrOyB9XHJcbiAgICAgICAgICAgICAgICAgICAgaWYgKHRbMl0pIF8ub3BzLnBvcCgpO1xyXG4gICAgICAgICAgICAgICAgICAgIF8udHJ5cy5wb3AoKTsgY29udGludWU7XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgb3AgPSBib2R5LmNhbGwodGhpc0FyZywgXyk7XHJcbiAgICAgICAgfSBjYXRjaCAoZSkgeyBvcCA9IFs2LCBlXTsgeSA9IDA7IH0gZmluYWxseSB7IGYgPSB0ID0gMDsgfVxyXG4gICAgICAgIGlmIChvcFswXSAmIDUpIHRocm93IG9wWzFdOyByZXR1cm4geyB2YWx1ZTogb3BbMF0gPyBvcFsxXSA6IHZvaWQgMCwgZG9uZTogdHJ1ZSB9O1xyXG4gICAgfVxyXG59XHJcblxyXG5leHBvcnQgdmFyIF9fY3JlYXRlQmluZGluZyA9IE9iamVjdC5jcmVhdGUgPyAoZnVuY3Rpb24obywgbSwgaywgazIpIHtcclxuICAgIGlmIChrMiA9PT0gdW5kZWZpbmVkKSBrMiA9IGs7XHJcbiAgICBPYmplY3QuZGVmaW5lUHJvcGVydHkobywgazIsIHsgZW51bWVyYWJsZTogdHJ1ZSwgZ2V0OiBmdW5jdGlvbigpIHsgcmV0dXJuIG1ba107IH0gfSk7XHJcbn0pIDogKGZ1bmN0aW9uKG8sIG0sIGssIGsyKSB7XHJcbiAgICBpZiAoazIgPT09IHVuZGVmaW5lZCkgazIgPSBrO1xyXG4gICAgb1trMl0gPSBtW2tdO1xyXG59KTtcclxuXHJcbmV4cG9ydCBmdW5jdGlvbiBfX2V4cG9ydFN0YXIobSwgbykge1xyXG4gICAgZm9yICh2YXIgcCBpbiBtKSBpZiAocCAhPT0gXCJkZWZhdWx0XCIgJiYgIU9iamVjdC5wcm90b3R5cGUuaGFzT3duUHJvcGVydHkuY2FsbChvLCBwKSkgX19jcmVhdGVCaW5kaW5nKG8sIG0sIHApO1xyXG59XHJcblxyXG5leHBvcnQgZnVuY3Rpb24gX192YWx1ZXMobykge1xyXG4gICAgdmFyIHMgPSB0eXBlb2YgU3ltYm9sID09PSBcImZ1bmN0aW9uXCIgJiYgU3ltYm9sLml0ZXJhdG9yLCBtID0gcyAmJiBvW3NdLCBpID0gMDtcclxuICAgIGlmIChtKSByZXR1cm4gbS5jYWxsKG8pO1xyXG4gICAgaWYgKG8gJiYgdHlwZW9mIG8ubGVuZ3RoID09PSBcIm51bWJlclwiKSByZXR1cm4ge1xyXG4gICAgICAgIG5leHQ6IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgaWYgKG8gJiYgaSA+PSBvLmxlbmd0aCkgbyA9IHZvaWQgMDtcclxuICAgICAgICAgICAgcmV0dXJuIHsgdmFsdWU6IG8gJiYgb1tpKytdLCBkb25lOiAhbyB9O1xyXG4gICAgICAgIH1cclxuICAgIH07XHJcbiAgICB0aHJvdyBuZXcgVHlwZUVycm9yKHMgPyBcIk9iamVjdCBpcyBub3QgaXRlcmFibGUuXCIgOiBcIlN5bWJvbC5pdGVyYXRvciBpcyBub3QgZGVmaW5lZC5cIik7XHJcbn1cclxuXHJcbmV4cG9ydCBmdW5jdGlvbiBfX3JlYWQobywgbikge1xyXG4gICAgdmFyIG0gPSB0eXBlb2YgU3ltYm9sID09PSBcImZ1bmN0aW9uXCIgJiYgb1tTeW1ib2wuaXRlcmF0b3JdO1xyXG4gICAgaWYgKCFtKSByZXR1cm4gbztcclxuICAgIHZhciBpID0gbS5jYWxsKG8pLCByLCBhciA9IFtdLCBlO1xyXG4gICAgdHJ5IHtcclxuICAgICAgICB3aGlsZSAoKG4gPT09IHZvaWQgMCB8fCBuLS0gPiAwKSAmJiAhKHIgPSBpLm5leHQoKSkuZG9uZSkgYXIucHVzaChyLnZhbHVlKTtcclxuICAgIH1cclxuICAgIGNhdGNoIChlcnJvcikgeyBlID0geyBlcnJvcjogZXJyb3IgfTsgfVxyXG4gICAgZmluYWxseSB7XHJcbiAgICAgICAgdHJ5IHtcclxuICAgICAgICAgICAgaWYgKHIgJiYgIXIuZG9uZSAmJiAobSA9IGlbXCJyZXR1cm5cIl0pKSBtLmNhbGwoaSk7XHJcbiAgICAgICAgfVxyXG4gICAgICAgIGZpbmFsbHkgeyBpZiAoZSkgdGhyb3cgZS5lcnJvcjsgfVxyXG4gICAgfVxyXG4gICAgcmV0dXJuIGFyO1xyXG59XHJcblxyXG5leHBvcnQgZnVuY3Rpb24gX19zcHJlYWQoKSB7XHJcbiAgICBmb3IgKHZhciBhciA9IFtdLCBpID0gMDsgaSA8IGFyZ3VtZW50cy5sZW5ndGg7IGkrKylcclxuICAgICAgICBhciA9IGFyLmNvbmNhdChfX3JlYWQoYXJndW1lbnRzW2ldKSk7XHJcbiAgICByZXR1cm4gYXI7XHJcbn1cclxuXHJcbmV4cG9ydCBmdW5jdGlvbiBfX3NwcmVhZEFycmF5cygpIHtcclxuICAgIGZvciAodmFyIHMgPSAwLCBpID0gMCwgaWwgPSBhcmd1bWVudHMubGVuZ3RoOyBpIDwgaWw7IGkrKykgcyArPSBhcmd1bWVudHNbaV0ubGVuZ3RoO1xyXG4gICAgZm9yICh2YXIgciA9IEFycmF5KHMpLCBrID0gMCwgaSA9IDA7IGkgPCBpbDsgaSsrKVxyXG4gICAgICAgIGZvciAodmFyIGEgPSBhcmd1bWVudHNbaV0sIGogPSAwLCBqbCA9IGEubGVuZ3RoOyBqIDwgamw7IGorKywgaysrKVxyXG4gICAgICAgICAgICByW2tdID0gYVtqXTtcclxuICAgIHJldHVybiByO1xyXG59O1xyXG5cclxuZXhwb3J0IGZ1bmN0aW9uIF9fYXdhaXQodikge1xyXG4gICAgcmV0dXJuIHRoaXMgaW5zdGFuY2VvZiBfX2F3YWl0ID8gKHRoaXMudiA9IHYsIHRoaXMpIDogbmV3IF9fYXdhaXQodik7XHJcbn1cclxuXHJcbmV4cG9ydCBmdW5jdGlvbiBfX2FzeW5jR2VuZXJhdG9yKHRoaXNBcmcsIF9hcmd1bWVudHMsIGdlbmVyYXRvcikge1xyXG4gICAgaWYgKCFTeW1ib2wuYXN5bmNJdGVyYXRvcikgdGhyb3cgbmV3IFR5cGVFcnJvcihcIlN5bWJvbC5hc3luY0l0ZXJhdG9yIGlzIG5vdCBkZWZpbmVkLlwiKTtcclxuICAgIHZhciBnID0gZ2VuZXJhdG9yLmFwcGx5KHRoaXNBcmcsIF9hcmd1bWVudHMgfHwgW10pLCBpLCBxID0gW107XHJcbiAgICByZXR1cm4gaSA9IHt9LCB2ZXJiKFwibmV4dFwiKSwgdmVyYihcInRocm93XCIpLCB2ZXJiKFwicmV0dXJuXCIpLCBpW1N5bWJvbC5hc3luY0l0ZXJhdG9yXSA9IGZ1bmN0aW9uICgpIHsgcmV0dXJuIHRoaXM7IH0sIGk7XHJcbiAgICBmdW5jdGlvbiB2ZXJiKG4pIHsgaWYgKGdbbl0pIGlbbl0gPSBmdW5jdGlvbiAodikgeyByZXR1cm4gbmV3IFByb21pc2UoZnVuY3Rpb24gKGEsIGIpIHsgcS5wdXNoKFtuLCB2LCBhLCBiXSkgPiAxIHx8IHJlc3VtZShuLCB2KTsgfSk7IH07IH1cclxuICAgIGZ1bmN0aW9uIHJlc3VtZShuLCB2KSB7IHRyeSB7IHN0ZXAoZ1tuXSh2KSk7IH0gY2F0Y2ggKGUpIHsgc2V0dGxlKHFbMF1bM10sIGUpOyB9IH1cclxuICAgIGZ1bmN0aW9uIHN0ZXAocikgeyByLnZhbHVlIGluc3RhbmNlb2YgX19hd2FpdCA/IFByb21pc2UucmVzb2x2ZShyLnZhbHVlLnYpLnRoZW4oZnVsZmlsbCwgcmVqZWN0KSA6IHNldHRsZShxWzBdWzJdLCByKTsgfVxyXG4gICAgZnVuY3Rpb24gZnVsZmlsbCh2YWx1ZSkgeyByZXN1bWUoXCJuZXh0XCIsIHZhbHVlKTsgfVxyXG4gICAgZnVuY3Rpb24gcmVqZWN0KHZhbHVlKSB7IHJlc3VtZShcInRocm93XCIsIHZhbHVlKTsgfVxyXG4gICAgZnVuY3Rpb24gc2V0dGxlKGYsIHYpIHsgaWYgKGYodiksIHEuc2hpZnQoKSwgcS5sZW5ndGgpIHJlc3VtZShxWzBdWzBdLCBxWzBdWzFdKTsgfVxyXG59XHJcblxyXG5leHBvcnQgZnVuY3Rpb24gX19hc3luY0RlbGVnYXRvcihvKSB7XHJcbiAgICB2YXIgaSwgcDtcclxuICAgIHJldHVybiBpID0ge30sIHZlcmIoXCJuZXh0XCIpLCB2ZXJiKFwidGhyb3dcIiwgZnVuY3Rpb24gKGUpIHsgdGhyb3cgZTsgfSksIHZlcmIoXCJyZXR1cm5cIiksIGlbU3ltYm9sLml0ZXJhdG9yXSA9IGZ1bmN0aW9uICgpIHsgcmV0dXJuIHRoaXM7IH0sIGk7XHJcbiAgICBmdW5jdGlvbiB2ZXJiKG4sIGYpIHsgaVtuXSA9IG9bbl0gPyBmdW5jdGlvbiAodikgeyByZXR1cm4gKHAgPSAhcCkgPyB7IHZhbHVlOiBfX2F3YWl0KG9bbl0odikpLCBkb25lOiBuID09PSBcInJldHVyblwiIH0gOiBmID8gZih2KSA6IHY7IH0gOiBmOyB9XHJcbn1cclxuXHJcbmV4cG9ydCBmdW5jdGlvbiBfX2FzeW5jVmFsdWVzKG8pIHtcclxuICAgIGlmICghU3ltYm9sLmFzeW5jSXRlcmF0b3IpIHRocm93IG5ldyBUeXBlRXJyb3IoXCJTeW1ib2wuYXN5bmNJdGVyYXRvciBpcyBub3QgZGVmaW5lZC5cIik7XHJcbiAgICB2YXIgbSA9IG9bU3ltYm9sLmFzeW5jSXRlcmF0b3JdLCBpO1xyXG4gICAgcmV0dXJuIG0gPyBtLmNhbGwobykgOiAobyA9IHR5cGVvZiBfX3ZhbHVlcyA9PT0gXCJmdW5jdGlvblwiID8gX192YWx1ZXMobykgOiBvW1N5bWJvbC5pdGVyYXRvcl0oKSwgaSA9IHt9LCB2ZXJiKFwibmV4dFwiKSwgdmVyYihcInRocm93XCIpLCB2ZXJiKFwicmV0dXJuXCIpLCBpW1N5bWJvbC5hc3luY0l0ZXJhdG9yXSA9IGZ1bmN0aW9uICgpIHsgcmV0dXJuIHRoaXM7IH0sIGkpO1xyXG4gICAgZnVuY3Rpb24gdmVyYihuKSB7IGlbbl0gPSBvW25dICYmIGZ1bmN0aW9uICh2KSB7IHJldHVybiBuZXcgUHJvbWlzZShmdW5jdGlvbiAocmVzb2x2ZSwgcmVqZWN0KSB7IHYgPSBvW25dKHYpLCBzZXR0bGUocmVzb2x2ZSwgcmVqZWN0LCB2LmRvbmUsIHYudmFsdWUpOyB9KTsgfTsgfVxyXG4gICAgZnVuY3Rpb24gc2V0dGxlKHJlc29sdmUsIHJlamVjdCwgZCwgdikgeyBQcm9taXNlLnJlc29sdmUodikudGhlbihmdW5jdGlvbih2KSB7IHJlc29sdmUoeyB2YWx1ZTogdiwgZG9uZTogZCB9KTsgfSwgcmVqZWN0KTsgfVxyXG59XHJcblxyXG5leHBvcnQgZnVuY3Rpb24gX19tYWtlVGVtcGxhdGVPYmplY3QoY29va2VkLCByYXcpIHtcclxuICAgIGlmIChPYmplY3QuZGVmaW5lUHJvcGVydHkpIHsgT2JqZWN0LmRlZmluZVByb3BlcnR5KGNvb2tlZCwgXCJyYXdcIiwgeyB2YWx1ZTogcmF3IH0pOyB9IGVsc2UgeyBjb29rZWQucmF3ID0gcmF3OyB9XHJcbiAgICByZXR1cm4gY29va2VkO1xyXG59O1xyXG5cclxudmFyIF9fc2V0TW9kdWxlRGVmYXVsdCA9IE9iamVjdC5jcmVhdGUgPyAoZnVuY3Rpb24obywgdikge1xyXG4gICAgT2JqZWN0LmRlZmluZVByb3BlcnR5KG8sIFwiZGVmYXVsdFwiLCB7IGVudW1lcmFibGU6IHRydWUsIHZhbHVlOiB2IH0pO1xyXG59KSA6IGZ1bmN0aW9uKG8sIHYpIHtcclxuICAgIG9bXCJkZWZhdWx0XCJdID0gdjtcclxufTtcclxuXHJcbmV4cG9ydCBmdW5jdGlvbiBfX2ltcG9ydFN0YXIobW9kKSB7XHJcbiAgICBpZiAobW9kICYmIG1vZC5fX2VzTW9kdWxlKSByZXR1cm4gbW9kO1xyXG4gICAgdmFyIHJlc3VsdCA9IHt9O1xyXG4gICAgaWYgKG1vZCAhPSBudWxsKSBmb3IgKHZhciBrIGluIG1vZCkgaWYgKGsgIT09IFwiZGVmYXVsdFwiICYmIE9iamVjdC5wcm90b3R5cGUuaGFzT3duUHJvcGVydHkuY2FsbChtb2QsIGspKSBfX2NyZWF0ZUJpbmRpbmcocmVzdWx0LCBtb2QsIGspO1xyXG4gICAgX19zZXRNb2R1bGVEZWZhdWx0KHJlc3VsdCwgbW9kKTtcclxuICAgIHJldHVybiByZXN1bHQ7XHJcbn1cclxuXHJcbmV4cG9ydCBmdW5jdGlvbiBfX2ltcG9ydERlZmF1bHQobW9kKSB7XHJcbiAgICByZXR1cm4gKG1vZCAmJiBtb2QuX19lc01vZHVsZSkgPyBtb2QgOiB7IGRlZmF1bHQ6IG1vZCB9O1xyXG59XHJcblxyXG5leHBvcnQgZnVuY3Rpb24gX19jbGFzc1ByaXZhdGVGaWVsZEdldChyZWNlaXZlciwgcHJpdmF0ZU1hcCkge1xyXG4gICAgaWYgKCFwcml2YXRlTWFwLmhhcyhyZWNlaXZlcikpIHtcclxuICAgICAgICB0aHJvdyBuZXcgVHlwZUVycm9yKFwiYXR0ZW1wdGVkIHRvIGdldCBwcml2YXRlIGZpZWxkIG9uIG5vbi1pbnN0YW5jZVwiKTtcclxuICAgIH1cclxuICAgIHJldHVybiBwcml2YXRlTWFwLmdldChyZWNlaXZlcik7XHJcbn1cclxuXHJcbmV4cG9ydCBmdW5jdGlvbiBfX2NsYXNzUHJpdmF0ZUZpZWxkU2V0KHJlY2VpdmVyLCBwcml2YXRlTWFwLCB2YWx1ZSkge1xyXG4gICAgaWYgKCFwcml2YXRlTWFwLmhhcyhyZWNlaXZlcikpIHtcclxuICAgICAgICB0aHJvdyBuZXcgVHlwZUVycm9yKFwiYXR0ZW1wdGVkIHRvIHNldCBwcml2YXRlIGZpZWxkIG9uIG5vbi1pbnN0YW5jZVwiKTtcclxuICAgIH1cclxuICAgIHByaXZhdGVNYXAuc2V0KHJlY2VpdmVyLCB2YWx1ZSk7XHJcbiAgICByZXR1cm4gdmFsdWU7XHJcbn1cclxuIiwibW9kdWxlLmV4cG9ydHMgPSB7XG4gIGxpYjogcmVxdWlyZSgnLi9lbW9qaXMnKSxcbiAgb3JkZXJlZDogcmVxdWlyZSgnLi9vcmRlcmVkJyksXG4gIGZpdHpwYXRyaWNrX3NjYWxlX21vZGlmaWVyczogW1wi8J+Pu1wiLCBcIvCfj7xcIiwgXCLwn4+9XCIsIFwi8J+PvlwiLCBcIvCfj79cIl1cbn1cbiIsImltcG9ydCB7IEFwcCwgRnV6enlTdWdnZXN0TW9kYWwsIFBsdWdpbiwgRnV6enlNYXRjaCwgTm90aWNlLCBNYXJrZG93blZpZXcgfSBmcm9tICdvYnNpZGlhbic7XG5pbXBvcnQgZW1vamkgZnJvbSAnZW1vamlsaWInO1xuXG5cbmNvbnN0IGluZGljYXRvclN0eWxlOiBzdHJpbmcgPVxuICAnY29sb3I6IHZhcigtLXRleHQtYWNjZW50KTsgd2lkdGg6IDIuNWVtOyB0ZXh0LWFsaWduOiBjZW50ZXI7IGZsb2F0OmxlZnQ7IGZvbnQtd2VpZ2h0OjgwMDsnO1xuXG5leHBvcnQgZGVmYXVsdCBjbGFzcyBNeVBsdWdpbiBleHRlbmRzIFBsdWdpbiB7XG4gIGVtb2ppczogRW1vamlJdGVtW11cblxuICBsb2FkRW1vamlzKCk6IEVtb2ppSXRlbVtdIHtcbiAgICBmdW5jdGlvbiB0aXRsZUNhc2Uoc3RyaW5nOiBzdHJpbmcpIHtcbiAgICAgIGxldCBzZW50ZW5jZSA9IHN0cmluZy50b0xvd2VyQ2FzZSgpLnNwbGl0KCdfJyk7XG4gICAgICBmb3IgKGxldCBpID0gMDsgaSA8IHNlbnRlbmNlLmxlbmd0aDsgaSsrKSB7XG4gICAgICAgIHNlbnRlbmNlW2ldID0gc2VudGVuY2VbaV1bMF0udG9VcHBlckNhc2UoKSArIHNlbnRlbmNlW2ldLnNsaWNlKDEpO1xuICAgICAgfVxuICBcbiAgICAgIHJldHVybiBzZW50ZW5jZS5qb2luKCcgJyk7XG4gICAgfVxuICBcbiAgICBsZXQgaXRlbXMgPSBlbW9qaS5vcmRlcmVkLm1hcCgobmFtZSkgPT4ge1xuICAgICAgcmV0dXJuIHtcbiAgICAgICAgbmFtZTogdGl0bGVDYXNlKG5hbWUpLFxuICAgICAgICBjaGFyOiBlbW9qaS5saWJbbmFtZV0uY2hhcixcbiAgICAgIH07XG4gICAgfSk7XG4gIFxuICAgIHJldHVybiBpdGVtcztcbiAgfVxuXG5cdG9ubG9hZCgpIHtcblxuICAgIHRoaXMuZW1vamlzID0gdGhpcy5sb2FkRW1vamlzKCk7XG5cblx0XHR0aGlzLmFkZENvbW1hbmQoe1xuXHRcdFx0aWQ6ICdlbW9qaS1waWNrZXI6b3Blbi1waWNrZXInLFxuICAgICAgbmFtZTogJ09wZW4gZW1vamkgcGlja2VyJyxcbiAgICAgIGhvdGtleXM6IFtdLFxuXHRcdFx0Y2hlY2tDYWxsYmFjazogKGNoZWNraW5nOiBib29sZWFuKSA9PiB7XG5cdFx0XHRcdGxldCBsZWFmID0gdGhpcy5hcHAud29ya3NwYWNlLmFjdGl2ZUxlYWY7XG5cdFx0XHRcdGlmIChsZWFmKSB7XG5cdFx0XHRcdFx0aWYgKCFjaGVja2luZykge1xuXHRcdFx0XHRcdFx0bmV3IEVtb2ppRnV6enlTdWdnZXN0TW9kYWwodGhpcy5hcHAsIHRoaXMuZW1vamlzKS5vcGVuKCk7XG5cdFx0XHRcdFx0fVxuXHRcdFx0XHRcdHJldHVybiB0cnVlO1xuXHRcdFx0XHR9XG5cdFx0XHRcdHJldHVybiBmYWxzZTtcblx0XHRcdH1cblx0XHR9KTtcblxuXHR9XG5cblx0b251bmxvYWQoKSB7XG5cdH1cbn1cblxuaW50ZXJmYWNlIEVtb2ppSXRlbSB7XG4gIG5hbWU6IHN0cmluZztcbiAgY2hhcjogc3RyaW5nO1xufVxuXG5cbmNsYXNzIEVtb2ppRnV6enlTdWdnZXN0TW9kYWwgZXh0ZW5kcyBGdXp6eVN1Z2dlc3RNb2RhbDxFbW9qaUl0ZW0+IHtcbiAgYXBwOiBBcHA7XG4gIGVtb2ppczogRW1vamlJdGVtW107XG5cbiAgY29uc3RydWN0b3IoYXBwOiBBcHAsIGVtb2ppczogRW1vamlJdGVtW10pIHtcbiAgICAgIHN1cGVyKGFwcCk7XG4gICAgICB0aGlzLmFwcCA9IGFwcDtcbiAgICAgIHRoaXMuZW1vamlzID0gZW1vamlzO1xuICB9XG5cbiAgZ2V0SXRlbXMoKTogRW1vamlJdGVtW10ge1xuICAgICAgcmV0dXJuIHRoaXMuZW1vamlzO1xuICB9XG5cbiAgZ2V0SXRlbVRleHQoaXRlbTogRW1vamlJdGVtKTogc3RyaW5nIHtcbiAgICAgIHJldHVybiBpdGVtLm5hbWU7XG4gIH1cblxuICByZW5kZXJTdWdnZXN0aW9uKGl0ZW06IEZ1enp5TWF0Y2g8RW1vamlJdGVtPiwgZWw6IEhUTUxFbGVtZW50KSB7XG4gICAgc3VwZXIucmVuZGVyU3VnZ2VzdGlvbihpdGVtLCBlbCk7XG4gICAgdGhpcy51cGRhdGVTdWdnZXN0aW9uRWxGb3JNb2RlKGl0ZW0sIGVsKTtcbiAgfVxuXG4gIHVwZGF0ZVN1Z2dlc3Rpb25FbEZvck1vZGUoaXRlbTogRnV6enlNYXRjaDxFbW9qaUl0ZW0+LCBlbDogSFRNTEVsZW1lbnQpIHtcblxuICAgIGNvbnN0IGluZGljYXRvckVsID0gY3JlYXRlRWwoJ2RpdicsIHtcbiAgICAgIHRleHQ6IGl0ZW0uaXRlbS5jaGFyLFxuICAgICAgYXR0cjogeyBzdHlsZTogaW5kaWNhdG9yU3R5bGUgfSxcbiAgICB9KTtcbiAgICBlbC5pbnNlcnRBZGphY2VudEVsZW1lbnQoJ2FmdGVyYmVnaW4nLCBpbmRpY2F0b3JFbCk7XG4gIH1cblxuICBpbnNlcnRUZXh0QXRDdXJzb3IodmlldzogTWFya2Rvd25WaWV3LCB0ZXh0OnN0cmluZyk6IHZvaWQge1xuICAgIGxldCBlZGl0b3IgPSB2aWV3LnNvdXJjZU1vZGUuY21FZGl0b3JcbiAgICBsZXQgZG9jID0gZWRpdG9yLmdldERvYygpO1xuICAgIGxldCBjdXJzb3IgPSBkb2MuZ2V0Q3Vyc29yKCk7XG4gICAgZG9jLnJlcGxhY2VSYW5nZSh0ZXh0LCBjdXJzb3IpO1xuICB9XG5cbiAgb25DaG9vc2VJdGVtKGl0ZW06IEVtb2ppSXRlbSwgZXZ0OiBNb3VzZUV2ZW50IHwgS2V5Ym9hcmRFdmVudCk6IHZvaWQge1xuICAgIGxldCBhY3RpdmVFZGl0b3IgPSB0aGlzLmFwcC53b3Jrc3BhY2UuZ2V0QWN0aXZlVmlld09mVHlwZShNYXJrZG93blZpZXcpXG4gICAgaWYgKGFjdGl2ZUVkaXRvcikge1xuICAgICAgdGhpcy5pbnNlcnRUZXh0QXRDdXJzb3IoYWN0aXZlRWRpdG9yLCBpdGVtLmNoYXIpXG4gICAgfSBlbHNlIHtcbiAgICAgIG5ldyBOb3RpY2UoXCJZb3UnbGwgbmVlZCB0byBvcGVuIGEgbWFya2Rvd24gZWRpdG9yIHRvIGluc2VydCBhbiBlbW9qaVwiKTtcbiAgICB9XG4gIH1cbn0iXSwibmFtZXMiOlsiZW1vamkiLCJQbHVnaW4iLCJNYXJrZG93blZpZXciLCJOb3RpY2UiLCJG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QUMsRUFBRTtBQUNoSCxJQUFJLE9BQU8sS0FBSyxDQUFDLEtBQUssQ0FBQyxHQUFHLE9BQU8sQ0FBQyxFQUFFLFVBQVUsT0FBTyxFQUFFLE1BQU0sRUFBRTtBQUMvRCxRQUFRLFNBQVMsU0FBUyxDQUFDLEtBQUssRUFBRSxFQUFFLElBQUksRUFBRSxJQUFJLENBQUMsU0FBUyxDQUFDLElBQUksQ0FBQyxLQUFLLENBQUMsQ0FBQyxDQUFDLEVBQUUsQ0FBQyxPQUFPLENBQUMsRUFBRSxFQUFFLE1BQU0sQ0FBQyxDQUFDLENBQUMsQ0FBQyxFQUFFLEVBQUU7QUFDbkcsUUFBUSxTQUFTLFFBQVEsQ0FBQyxLQUFLLEVBQUUsRUFBRSxJQUFJLEVBQUUsSUFBSSxDQUFDLFNBQVMsQ0FBQyxPQUFPLENBQUMsQ0FBQyxLQUFLLENBQUMsQ0FBQyxDQUFDLEVBQUUsQ0FBQyxPQUFPLENBQUMsRUFBRSxFQUFFLE1BQU0sQ0FBQyxDQUFDLENBQUMsQ0FBQyxFQUFFLEVBQUU7QUFDdEcsUUFBUSxTQUFTLElBQUksQ0FBQyxNQUFNLEVBQUUsRUFBRSxNQUFNLENBQUMsSUFBSSxHQUFHLE9BQU8sQ0FBQyxNQUFNLENBQUMsS0FBSyxDQUFDLEdBQUcsS0FBSyxDQUFDLE1BQU0sQ0FBQyxLQUFLLENBQUMsQ0FBQyxJQUFJLENBQUMsU0FBUyxFQUFFLFFBQVEsQ0FBQyxDQUFDLEVBQUU7QUFDdEgsUUFBUSxJQUFJLENBQUMsQ0FBQyxTQUFTLEdBQUcsU0FBUyxDQUFDLEtBQUssQ0FBQyxPQUFPLEVBQUUsVUFBVSxJQUFJLEVBQUUsQ0FBQyxFQUFFLElBQUksRUFBRSxDQUFDLENBQUM7QUFDOUUsS0FBSyxDQUFDLENBQUM7QUFDUCxDQUFDO0FBQ0Q7QUFDTyxTQUFTLFdBQVcsQ0FBQyxPQUFPLEVBQUUsSUFBSSxFQUFFO0FBQzNDLElBQUksSUFBSSxDQUFDLEdBQUcsRUFBRSxLQUFLLEVBQUUsQ0FBQyxFQUFFLElBQUksRUFBRSxXQUFXLEVBQUUsSUFBSSxDQUFDLENBQUMsQ0FBQyxDQUFDLEdBQUcsQ0FBQyxFQUFFLE1BQU0sQ0FBQyxDQUFDLENBQUMsQ0FBQyxDQUFDLENBQUMsT0FBTyxDQUFDLENBQUMsQ0FBQyxDQUFDLENBQUMsRUFBRSxFQUFFLElBQUksRUFBRSxFQUFFLEVBQUUsR0FBRyxFQUFFLEVBQUUsRUFBRSxFQUFFLENBQUMsRUFBRSxDQUFDLEVBQUUsQ0FBQyxFQUFFLENBQUMsQ0FBQztBQUNySCxJQUFJLE9BQU8sQ0FBQyxHQUFHLEVBQUUsSUFBSSxFQUFFLElBQUksQ0FBQyxDQUFDLENBQUMsRUFBRSxPQUFPLEVBQUUsSUFBSSxDQUFDLENBQUMsQ0FBQyxFQUFFLFFBQVEsRUFBRSxJQUFJLENBQUMsQ0FBQyxDQUFDLEVBQUUsRUFBRSxPQUFPLE1BQU0sS0FBSyxVQUFVLEtBQUssQ0FBQyxDQUFDLE1BQU0sQ0FBQyxRQUFRLENBQUMsR0FBRyxXQUFXLEVBQUUsT0FBTyxJQUFJLENBQUMsRUFBRSxDQUFDLEVBQUUsQ0FBQyxDQUFDO0FBQzdKLElBQUksU0FBUyxJQUFJLENBQUMsQ0FBQyxFQUFFLEVBQUUsT0FBTyxVQUFVLENBQUMsRUFBRSxFQUFFLE9BQU8sSUFBSSxDQUFDLENBQUMsQ0FBQyxFQUFFLENBQUMsQ0FBQyxDQUFDLENBQUMsRUFBRSxDQUFDLEVBQUU7QUFDdEUsSUFBSSxTQUFTLElBQUksQ0FBQyxFQUFFLEVBQUU7QUFDdEIsUUFBUSxJQUFJLENBQUMsRUFBRSxNQUFNLElBQUksU0FBUyxDQUFDLGlDQUFpQyxDQUFDLENBQUM7QUFDdEUsUUFBUSxPQUFPLENBQUMsRUFBRSxJQUFJO0FBQ3RCLFlBQVksSUFBSSxDQUFDLEdBQUcsQ0FBQyxFQUFFLENBQUMsS0FBSyxDQUFDLEdBQUcsRUFBRSxDQUFDLENBQUMsQ0FBQyxHQUFHLENBQUMsR0FBRyxDQUFDLENBQUMsUUFBUSxDQUFDLEdBQUcsRUFBRSxDQUFDLENBQUMsQ0FBQyxHQUFHLENBQUMsQ0FBQyxPQUFPLENBQUMsS0FBSyxDQUFDLENBQUMsR0FBRyxDQUFDLENBQUMsUUFBUSxDQUFDLEtBQUssQ0FBQyxDQUFDLElBQUksQ0FBQyxDQUFDLENB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UyxnQkFBZ0IsQ0FBQyxPQUFPLEVBQUUsVUFBVSxFQUFFLFNBQVMsRUFBRTtBQUNqRSxJQUFJLElBQUksQ0FBQyxNQUFNLENBQUMsYUFBYSxFQUFFLE1BQU0sSUFBSSxTQUFTLENBQUMsc0NBQXNDLENBQUMsQ0FBQztBQUMzRixJQUFJLElBQUksQ0FBQyxHQUFHLFNBQVMsQ0FBQyxLQUFLLENBQUMsT0FBTyxFQUFFLFVBQVUsSUFBSSxFQUFFLENBQUMsRUFBRSxDQUFDLEVBQUUsQ0FBQyxHQUFHLEVBQUUsQ0FBQztBQUNsRSxJQUFJLE9BQU8sQ0FBQyxHQUFHLEVBQUUsRUFBRSxJQUFJLENBQUMsTUFBTSxDQUFDLEVBQUUsSUFBSSxDQUFDLE9BQU8sQ0FBQyxFQUFFLElBQUksQ0FBQyxRQUFRLENBQUMsRUFBRSxDQUFDLENBQUMsTUFBTSxDQUFDLGFBQWEsQ0FBQyxHQUFHLFlBQVksRUFBRSxPQUFPLElBQUksQ0FBQyxFQUFFLEVBQUUsQ0FBQyxDQUFDO0FBQzFILElBQUksU0FBUyxJQUFJLENBQUMsQ0FBQyxFQUFFLEVBQUUsSUFBSSxDQUFDLENBQUMsQ0FBQyxDQUFDLEVBQUUsQ0FBQyxDQUFDLENBQUMsQ0FBQyxHQUFHLFVBQVUsQ0FBQyxFQUFFLEVBQUUsT0FBTyxJQUFJLE9BQU8sQ0FBQyxVQUFVLENBQUMsRUFBRSxDQUFDLEVBQUUsRUFBRSxDQUFDLENBQUMsSUFBSSxDQUFDLENBQUMsQ0FBQyxFQUFFLENBQUMsRUFBRSxDQUFDLEVBQUUsQ0FBQyxDQUFDLENBQUMsR0FBRyxDQUFDLElBQUksTUFBTSxDQUFDLENBQUMsRUFBRSxDQUFDLENBQUMsQ0FBQyxFQUFFLENBQUMsQ0FBQyxFQUFFLENBQUMsRUFBRTtBQUM5SSxJQUFJLFNBQVMsTUFBTSxDQUFDLENBQUMsRUFBRSxDQUFDLEVBQUUsRUFBRSxJQUFJLEVBQUUsSUFBSSxDQUFDLENBQUMsQ0FBQyxDQUFDLENBQUMsQ0FBQyxDQUFDLENBQUMsQ0FBQyxDQUFDLEVBQUUsQ0FBQyxPQUFPLENBQUMsRUFBRSxFQUFFLE1BQU0sQ0FBQyxDQUFDLENBQUMsQ0FBQyxDQUFDLENBQUMsQ0FBQyxDQUFDLEVBQUUsQ0FBQyxDQUFDLENBQUMsRUFBRSxFQUFFO0FBQ3RGLElBQUksU0FBUyxJQUFJLENBQUMsQ0FBQyxFQUFFLEVBQUUsQ0FBQyxDQUFDLEtBQUssWUFBWSxPQUFPLEdBQUcsT0FBTyxDQUFDLE9BQU8sQ0FBQyxDQUFDLENBQUMsS0FBSyxDQUFDLENBQUMsQ0FBQyxDQUFDLElBQUksQ0FBQyxPQUFPLEVBQUUsTUFBTSxDQUFDLEdBQUcsTUFBTSxDQUFDLENBQUMsQ0FBQyxDQUFDLENBQUMsQ0FBQyxDQUFDLENBQUMsRUFBRSxDQUFDLENBQUMsQ0FBQyxFQUFFO0FBQzVILElBQUksU0FBUyxPQUFPLENBQUMsS0FBSyxFQUFFLEVBQUUsTUFBTSxDQUFDLE1BQU0sRUFBRSxLQUFLLENBQUMsQ0FBQyxFQUFFO0FBQ3RELElBQUksU0FBUyxNQUFNLENBQUMsS0FBSyxFQUFFLEVBQUUsTUFBTSxDQUFDLE9BQU8sRUFBRSxLQUFLLENBQUMsQ0FBQyxFQUFFO0FBQ3RELElBQUksU0FBUyxNQUFNLENBQUMsQ0FBQyxFQUFFLENBQUMsRUFBRSxFQUFFLElBQUksQ0FBQyxDQUFDLENBQUMsQ0FBQyxFQUFFLENBQUMsQ0FBQyxLQUFLLEVBQUUsRUFBRSxDQUFDLENBQUMsTUFBTSxFQUFFLE1BQU0sQ0FBQyxDQUFDLENBQUMsQ0FBQyxDQUFDLENBQUMsQ0FBQyxDQUFDLEVBQUUsQ0FBQyxDQUFDLENBQUMsQ0FBQyxDQUFDLENBQUMsQ0FBQyxDQUFDLENBQUMsRUFBRTtBQUN0RixDQUFDO0FBQ0Q7QUFDTyxTQUFTLGdCQUFnQixDQUFDLENBQUMsRUFBRTtBQUNwQyxJQUFJLElBQUksQ0FBQyxFQUFFLENBQUMsQ0FBQztBQUNiLElBQUksT0FBTyxDQUFDLEdBQUcsRUFBRSxFQUFFLElBQUksQ0FBQyxNQUFNLENBQUMsRUFBRSxJQUFJLENBQUMsT0FBTyxFQUFFLFVBQVUsQ0FBQyxFQUFFLEVBQUUsTUFBTSxDQUFDLENBQUMsRUFBRSxDQUFDLEVBQUUsSUFBSSxD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Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7OztBQ2xPQSxZQUFjLEdBQUc7QUFDakIsRUFBRSxHQUFHLEVBQUUsVUFBbUI7QUFDMUIsRUFBRSxPQUFPLEVBQUUsVUFBb0I7QUFDL0IsRUFBRSwyQkFBMkIsRUFBRSxDQUFDLElBQUksRUFBRSxJQUFJLEVBQUUsSUFBSSxFQUFFLElBQUksRUFBRSxJQUFJLENBQUM7QUFDN0Q7O0FDQUEsSUFBTSxjQUFjLEdBQ2xCLDJGQUEyRixDQUFDOztJQUV4RCw0QkFBTTtJQUE1Qzs7S0ErQ0M7SUE1Q0MsNkJBQVUsR0FBVjtRQUNFLFNBQVMsU0FBUyxDQUFDLE1BQWM7WUFDL0IsSUFBSSxRQUFRLEdBQUcsTUFBTSxDQUFDLFdBQVcsRUFBRSxDQUFDLEtBQUssQ0FBQyxHQUFHLENBQUMsQ0FBQztZQUMvQyxLQUFLLElBQUksQ0FBQyxHQUFHLENBQUMsRUFBRSxDQUFDLEdBQUcsUUFBUSxDQUFDLE1BQU0sRUFBRSxDQUFDLEVBQUUsRUFBRTtnQkFDeEMsUUFBUSxDQUFDLENBQUMsQ0FBQyxHQUFHLFFBQVEsQ0FBQyxDQUFDLENBQUMsQ0FBQyxDQUFDLENBQUMsQ0FBQyxXQUFXLEVBQUUsR0FBRyxRQUFRLENBQUMsQ0FBQyxDQUFDLENBQUMsS0FBSyxDQUFDLENBQUMsQ0FBQyxDQUFDO2FBQ25FO1lBRUQsT0FBTyxRQUFRLENBQUMsSUFBSSxDQUFDLEdBQUcsQ0FBQyxDQUFDO1NBQzNCO1FBRUQsSUFBSSxLQUFLLEdBQUdBLFFBQUssQ0FBQyxPQUFPLENBQUMsR0FBRyxDQUFDLFVBQUMsSUFBSTtZQUNqQyxPQUFPO2dCQUNMLElBQUksRUFBRSxTQUFTLENBQUMsSUFBSSxDQUFDO2dCQUNyQixJQUFJLEVBQUVBLFFBQUssQ0FBQyxHQUFHLENBQUMsSUFBSSxDQUFDLENBQUMsSUFBSTthQUMzQixDQUFDO1NBQ0gsQ0FBQyxDQUFDO1FBRUgsT0FBTyxLQUFLLENBQUM7S0FDZDtJQUVGLHlCQUFNLEdBQU47UUFBQSxpQkFvQkM7UUFsQkUsSUFBSSxDQUFDLE1BQU0sR0FBRyxJQUFJLENBQUMsVUFBVSxFQUFFLENBQUM7UUFFbEMsSUFBSSxDQUFDLFVBQVUsQ0FBQztZQUNmLEVBQUUsRUFBRSwwQkFBMEI7WUFDM0IsSUFBSSxFQUFFLG1CQUFtQjtZQUN6QixPQUFPLEVBQUUsRUFBRTtZQUNkLGFBQWEsRUFBRSxVQUFDLFFBQWlCO2dCQUNoQyxJQUFJLElBQUksR0FBRyxLQUFJLENBQUMsR0FBRyxDQUFDLFNBQVMsQ0FBQyxVQUFVLENBQUM7Z0JBQ3pDLElBQUksSUFBSSxFQUFFO29CQUNULElBQUksQ0FBQyxRQUFRLEVBQUU7d0JBQ2QsSUFBSSxzQkFBc0IsQ0FBQyxLQUFJLENBQUMsR0FBRyxFQUFFLEtBQUksQ0FBQyxNQUFNLENBQUMsQ0FBQyxJQUFJLEVBQUUsQ0FBQztxQkFDekQ7b0JBQ0QsT0FBTyxJQUFJLENBQUM7aUJBQ1o7Z0JBQ0QsT0FBTyxLQUFLLENBQUM7YUFDYjtTQUNELENBQUMsQ0FBQztLQUVIO0lBRUQsMkJBQVEsR0FBUjtLQUNDO0lBQ0YsZUFBQztBQUFELENBL0NBLENBQXNDQyxlQUFNLEdBK0MzQztBQVFEO0lBQXFDLDBDQUE0QjtJQUkvRCxnQ0FBWSxHQUFRLEVBQUUsTUFBbUI7UUFBekMsWUFDSSxrQkFBTSxHQUFHLENBQUMsU0FHYjtRQUZHLEtBQUksQ0FBQyxHQUFHLEdBQUcsR0FBRyxDQUFDO1FBQ2YsS0FBSSxDQUFDLE1BQU0sR0FBRyxNQUFNLENBQUM7O0tBQ3hCO0lBRUQseUNBQVEsR0FBUjtRQUNJLE9BQU8sSUFBSSxDQUFDLE1BQU0sQ0FBQztLQUN0QjtJQUVELDRDQUFXLEdBQVgsVUFBWSxJQUFlO1FBQ3ZCLE9BQU8sSUFBSSxDQUFDLElBQUksQ0FBQztLQUNwQjtJQUVELGlEQUFnQixHQUFoQixVQUFpQixJQUEyQixFQUFFLEVBQWU7UUFDM0QsaUJBQU0sZ0JBQWdCLFlBQUMsSUFBSSxFQUFFLEVBQUUsQ0FBQyxDQUFDO1FBQ2pDLElBQUksQ0FBQyx5QkFBeUIsQ0FBQyxJQUFJLEVBQUUsRUFBRSxDQUFDLENBQUM7S0FDMUM7SUFFRCwwREFBeUIsR0FBekIsVUFBMEIsSUFBMkIsRUFBRSxFQUFlO1FBRXBFLElBQU0sV0FBVyxHQUFHLFFBQVEsQ0FBQyxLQUFLLEVBQUU7WUFDbEMsSUFBSSxFQUFFLElBQUksQ0FBQyxJQUFJLENBQUMsSUFBSTtZQUNwQixJQUFJLEVBQUUsRUFBRSxLQUFLLEVBQUUsY0FBYyxFQUFFO1NBQ2hDLENBQUMsQ0FBQztRQUNILEVBQUUsQ0FBQyxxQkFBcUIsQ0FBQyxZQUFZLEVBQUUsV0FBVyxDQUFDLENBQUM7S0FDckQ7SUFFRCxtREFBa0IsR0FBbEIsVUFBbUIsSUFBa0IsRUFBRSxJQUFXO1FBQ2hELElBQUksTUFBTSxHQUFHLElBQUksQ0FBQyxVQUFVLENBQUMsUUFBUSxDQUFBO1FBQ3JDLElBQUksR0FBRyxHQUFHLE1BQU0sQ0FBQyxNQUFNLEVBQUUsQ0FBQztRQUMxQixJQUFJLE1BQU0sR0FBRyxHQUFHLENBQUMsU0FBUyxFQUFFLENBQUM7UUFDN0IsR0FBRyxDQUFDLFlBQVksQ0FBQyxJQUFJLEVBQUUsTUFBTSxDQUFDLENBQUM7S0FDaEM7SUFFRCw2Q0FBWSxHQUFaLFVBQWEsSUFBZSxFQUFFLEdBQStCO1FBQzNELElBQUksWUFBWSxHQUFHLElBQUksQ0FBQyxHQUFHLENBQUMsU0FBUyxDQUFDLG1CQUFtQixDQUFDQyxxQkFBWSxDQUFDLENBQUE7UUFDdkUsSUFBSSxZQUFZLEVBQUU7WUFDaEIsSUFBSSxDQUFDLGtCQUFrQixDQUFDLFlBQVksRUFBRSxJQUFJLENBQUMsSUFBSSxDQUFDLENBQUE7U0FDakQ7YUFBTTtZQUNMLElBQUlDLGVBQU0sQ0FBQywwREFBMEQsQ0FBQyxDQUFDO1NBQ3hFO0tBQ0Y7SUFDSCw2QkFBQztBQUFELENBL0NBLENBQXFDQywwQkFBaUI7Ozs7In0=

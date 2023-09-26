using BitcoinCash;
using BitcoinCash.Models;
using Newtonsoft.Json;

var client = new BitcoinCashClient();

// create new wallet
var wallet = client.GetWallet();
Display("new wallet", wallet);

// get sample (existing) wallet
// to add to sample wallet balance, send BCH here: bitcoincash:qrkvgur05slhujutthx04mzc0mc483p7lqca5cr4qe
var privateKey = "L2ES4zx4AnTYTQWQnuUvzBEwsXdhaoLi9W15HgKw8osVaxeUHbcE";
wallet = client.GetWallet(privateKey);
Display("sample wallet", wallet);

// send 10% of wallet balance to sample address
var sampleAddress = Constants.DevAddress;
var sendAmount = (decimal)(wallet.Balance! * 0.1);
wallet.Send(sampleAddress, sendAmount, Currency.Satoshis);
Display("wallet after sat send", wallet);

// send $0.10 to sample address
sendAmount = 0.1m;
wallet.Send(sampleAddress, sendAmount, Currency.USDollar);
Display("wallet after usd send", wallet);

// get read-only wallet by public address
var samplePublicAddress = Constants.DevAddress;
var readOnlyWallet = client.GetWalletByAddress(samplePublicAddress);
Display("read-only wallet", readOnlyWallet);

// get current market value of BCH in default currency (USD)
var usdValue = client.GetFiatValue();
Display("USD value", usdValue);

// get current market value of BCH in other currencies
var euroValue = client.GetFiatValue(Currency.Euro);
Display("Euro value", euroValue);

var yuanValue = client.GetFiatValue(Currency.ChineseYuan);
Display("Yuan value", yuanValue);

var pesoValue = client.GetFiatValue(Currency.MexicanPeso);
Display("Peso value", pesoValue);

// use a different base fiat currency
var clientOptions = new ClientOptions
{
    Currency = Currency.SouthAfricanRand
};

client = new BitcoinCashClient(clientOptions);

// wallet inherits currency of client
wallet = client.GetWallet(privateKey);
Display("South African Rand wallet", wallet);

// send using base fiat currency
wallet.Send(Constants.DevAddress, 3m, Currency.SouthAfricanRand);
Display("South African Rand wallet after ZAR send", wallet);

// send using non-base fiat currency
wallet.Send(Constants.DevAddress, 0.1m, Currency.Euro);
Display("South African Rand wallet after Euro send", wallet);

//check the balances of many wallets 
var addresses = new List<string>
{
    "bitcoincash:qz8mktu2r7jytlcldga3mwljcn99y52wwstq0aex3f",
    "bitcoincash:qr6wxzul24eqcq2jqjd0sg8wggnwc8llqyhhgwva75",
    "bitcoincash:qzmxdaglmhps28fs37qgz6uk5guldfdw2guguglh38",
    "bitcoincash:qqlfwy3z42xem72472p4kles9thayayj8qscpas9mk",
    "bitcoincash:qzgzfqfsjx76k8jswlef2q9g0gqvfjfg5cwf355qhz",
    "bitcoincash:qrg0d30486u93u85gvqgdljl4xy8zsut5vgrjk3d0y",
    "bitcoincash:qrfxul8ppuzpehcsql5s2u59pq0glpp86vpf3nk87f",
    "bitcoincash:qz9s9s70g6ldxw726cf7epquyk64ddffwy39knyv76",
    "bitcoincash:qpz49xqawfn6832zqn46y0xae3dw662d5v8havr68l",
    "bitcoincash:qqh8n4jg5w4x35gwemkhf3j3ke5avtwx3gymq8u90q",
    "bitcoincash:qpdvm4wwvadm5xmr95gf6u5ad9lxlx80hsu7z0k7kx",
    "bitcoincash:qq60yzxv54ywgudv3mzd6njy3f3a2sdvdyjp27sgde",
    "bitcoincash:qqn5995240mfnz8p6reg7u3u89ls90sg8gvs4hqdf2",
    "bitcoincash:qq79azfhvznjzt2l06naplqv40zj39mv5q0jd42dah",
    "bitcoincash:qzksvs507eh4dg64526vnzexkgnaxlcu9shczn9dv6",
    "bitcoincash:qqq3n9kk0g4r2u0w9qvft76x9v25d7lacchaudgupg",
    "bitcoincash:qzt262suylwafq0dqpmdlej0hfj2q8f55vdu6e7pgm",
    "bitcoincash:qpz88v4p06zgxluu3zv7q230e7nvxmjshy5fr24tx4",
    "bitcoincash:qqdrmq7dj3qc7pzrytfs5yg57z8qsx4tly9dn96zln",
    "bitcoincash:qr8wwc6l94f9cc7jt8pv936pz8nqjggjjulvxw6e6a",
    "bitcoincash:qpvgts755nrh8cquhgj25smdduvkespnqqahh7wvgp",
    "bitcoincash:qqlss80d5fdla94xyrra6lqdyl4nhxpf4gm3c9pxd3",
    "bitcoincash:qrjrpm4m0v0v099ahuqf9r82tvmxd7qkvvhz62q4ee",
    "bitcoincash:qpqlegkk6j3u6umsk00m2ynr2hgv30avr5hz2rt2y7",
    "bitcoincash:qzg3jnd7cf9yyl97llsljex8c7qkhna90sqere593p",
    "bitcoincash:qpzus77rz2pjdq6j3n9dmmc3py0ydz6e45dnulnxpx",
    "bitcoincash:qrdvce2xesghudw55f8gqhlfyhljvm6wxy8wa9nuyz",
    "bitcoincash:qqkwzupgmvqlv7xg3la6wdq95esu8m9t5vhzudghnc",
    "bitcoincash:qpgmgqz8yq3xm4yk9kh28wxh45dnfly56u00s9slez",
    "bitcoincash:qzr2tr0slft8s9ytmdhy6enrxavk79n03uykjzxl9h",
    "bitcoincash:qqff4zzargssgg49l0cd0s83vg8zq2ja4qpglxpv37",
    "bitcoincash:qrqnf57xpt6c3vzlwcr3lef6xr668zr9nyskmpmjpl",
    "bitcoincash:qp0z6l5w4sar7v79devpj8wcy7wmakx9yvqx6wrrht",
    "bitcoincash:qr5yv9a6ncqgdyk35jdaap3mtt7avx5alqzfl8yh5z",
    "bitcoincash:qzcu4wmzn5rfysrv9n4zcny75wwhrm044yn8z0xpvt",
    "bitcoincash:qp7tswat43p0g7975n79dlf7yfvfml2p8q6t0tl5h4",
    "bitcoincash:qqxvwyugh8vkamg96kl3stkkj2vpjs4gjvc55k2kdh",
    "bitcoincash:qprnxx95h2v9g2wlmzmnsf0dt45t5gcjpye6zhx5h4",
    "bitcoincash:qz3jdz4hxcf0tqf9hhx88jswvd0nnxq7m5jq6preft",
    "qrylzkswtldcspkr085cgs0g8nantghjrvrvly9gyr",
    "bitcoincash:qz2ak94fkjlczxua4pvu8tnxu8hkqncgw5psacgaag",
    "bitcoincash:qqyvyj7kmccvcvrtrvdxxz8f409jfu0jdqn503znj3",
    "bitcoincash:qpw0qvwufljlvtmadlek5jusq5j47fexm5tus2r7ua",
    "bitcoincash:qqku5jfcvs728f7xqxdn884w6s5zd4w8vy68uykw67",
    "bitcoincash:qr8pjn3kakkwvxw0mfun983673qpwrfhlcwluvp428",
    "bitcoincash:qp8sh9836jjvux6cz986r5ewvx56n0cszygtrktfqy",
    "bitcoincash:qp8wu73wac45vm0q6pvs9kk3n2gmn689tg927dwm69",
    "bitcoincash:qqr6gm9wmny6yrs0zx64536ddm6v39sd5g7x4r4v7w",
    "bitcoincash:qqu56pzewatswas2y9d2a9jkt2eyk2jrwu9xlk0gmp",
    "bitcoincash:qrwyyclkpdekrguz0x6fdmcj2nxyckl3hsp53e98kd",
    "bitcoincash:qqhcg9emhpfxkehats450a52pf5r84cvk58gjqgsh2",
    "bitcoincash:qzxhvq32svzh94ne8jr5xdkqhjr5hqcegvjnfl684s",
    "bitcoincash:qp67a30mfzp8asag24l3gdnrkt4ujywahveq7udt59",
    "bitcoincash:qpuxjnjsyrqv5x8mrdcd4p8tksnchcjd8qdhhfz7qh",
    "bitcoincash:qpxxn7u3rzcuzf5zhpkjueseep2zyw0kay4vwcyaux",
    "bitcoincash:qp59q4y54xcdk8fmz5psqemjrj4yeghmpuyhv7vruf",
    "bitcoincash:qzauk3kqev79hytm6p52qesvej3y3y242yhfc2ydqc",
    "bitcoincash:qz0s26d5jdvfjx5zam7u8nm38xssgu6djv7tm9syl6",
    "bitcoincash:qpaztlsmgsf8x3xyjgsrsfwdxsp5jcf9tsvdsm0qd7",
    "bitcoincash:qrxt7vzfe8q24zwt807gkvj8adzjmapecv7hqlk0mg",
    "bitcoincash:qpvtpzprk86j6kktwxsdghqtlpsazhp0e5ha4jgpv6",
    "bitcoincash:qr36nv0m8ylxfcj7tekeq8h2v6qv4ey75cz3gq4ye6",
    "bitcoincash:qzhc8nze82d90q06zzcq5nfp5dwpufdxhy9h5g5z3e",
    "bitcoincash:qz8cqc802dwjpqvm92em9ee0faz9v23uf5mles2hc3",
    "bitcoincash:qrgu04r9qcw9x973mtxphs00fhx4ylfzcyne3rx8zj",
    "bitcoincash:qpmvvu3p8727lwnxax32vq22laflgzydl5hy59nsl0",
    "invalidAddress",
    "bitcoincash:qz2u02qm9edq9c5ls5h9q9gfyhjzcv29nchxz75jsx",
    "bitcoincash:qzerlc6cvs4w9zv4kazjs9hhlq4jrjrf3suldlucck",
    "bitcoincash:qqmg2cj4egz9234autpavnpt4mm3g9a8nc4kml0xep",
    "bitcoincash:qqs2kct0628zy074ad329qgkee2zlzuywcalah48g8",
    "bitcoincash:qrs7depsz4ckgleryeaagz3s9am7sxq7sg2d8cxjk5",
    "bitcoincash:qz4xn8xm5u9vzp7q87pts207tpxhnzxw3s0j53nu3g",
    "bitcoincash:qrelzx5885a2r7nca0qs53dc3wex5yfyn5qcgcujjc",
    "qpew0ls6gg53z8l44zsy0fyqejuvdfmdxu49dzfx3n",
    "bitcoincash:qqlc5hdrx6qg6pl7zhmpkmxtl70aw849h5pcj3wtal",
    "bitcoincash:qr2aq62f225twmsmd3rcldvrlkt0t37dfvk8dmd8ah",
    "bitcoincash:qzj7hpag8wsr3j2ghv3u55gcpvdmcfjlqgzxza6ssw",
    "bitcoincash:qprqkqjf0t2vr4kqt8eygrxfed4www2xzuh7wrc9um",
    "bitcoincash:qzpttlxcdljfwmgntekjqdz8x5c5uxkecg3yd0l0c8",
    "bitcoincash:qpwv2zrjmd8jx9m8zdcae48j7ggdw8mapqntewanh6",
    "bitcoincash:qz9vx5yfr4xmpj8n3jxl758hnj9ns8ed0up92agju4",
    "bitcoincash:qqq5lrn3ltdf722wcg09j8tqj8gmg0w8nc7q8sr43r",
    "bitcoincash:qqwqeztxwu9j4evcnx3x898gu7t3q6vce58kppqfx6",
    "bitcoincash:qq7ctsrv50tfshjnmwn2uz2dw59zl4ywxv89seuj0z",
    "bitcoincash:qq8lfcel9rh23w05544zznckr4wh3rgugux046wfy5",
    "bitcoincash:qq4wumlrms799pgr0h7ccfmc84468rzcrv5ljh9ssj",
    "bitcoincash:qrgev5atfsvg2s8v7xtwls9qwycn6j9facw0pyql8d",
    "bitcoincash:qq9jzx02r0d9h0m6nc0vh9faq77aac8n0c5vstsdls",
    "anotherInvalidAddress",
    "bitcoincash:qryt7pfhdz0sstwwv8c2mk3hlqq49yr4tc4yuqj3y6",
    "bitcoincash:qrkzkf23z0wspnr6gy8dera3073s7lsm0gx4nq0s4m",
    "bitcoincash:qr75g3zxun5z97tept4fuw4fjgwg89sng5ljpjfpdu",
    "bitcoincash:qr25rmqjulqsqwkytcqrx3yr372424j3tu0rykr9yn",
    "bitcoincash:qrt8nr4px6sf6r6d3dg6c54g3j4rjlwttgjk5erf2j",
    "bitcoincash:qzrdnule6f7qucv8cp0mrz9mj5wnj8s57cwk742ry5",
    "bitcoincash:qqd95x7mst7yuqvpcr78nvhr7k0p682qzg4pquj0ql",
    "bitcoincash:qzlepawgkuzuxklyuk40ut3w3kenjzdyfyht7yq6vg",
    "bitcoincash:qrcvecteyud062y45lzq45ar8ukpxgz9pqmaqjsa4m",
    "bitcoincash:qps7h9zszq66v9jx0kkshplntyv2ygjtmuzsm2q4vf",
    "bitcoincash:qrpgtfw7l3weqcvw5lcrqeed8j7lg3r6tych0cessh",
    "bitcoincash:qqjd2f0mpktkhxjq3u7hnd3zr4205vlg3y4qsg42cq",
    "bitcoincash:qp0ldgpkk8382mhjz0hrrwm03jvpzcg25ys6npj9tv",
    "bitcoincash:qphdcy4gjh96qx3nkq28xns4rsg5m236q5gahe38zq",
    "bitcoincash:qqj9j08c70sds0e0sdvx7dgq2328am2fvclpdulu7e",
    "bitcoincash:qzv7x36zg3u7n0vvuuxzpr697crt6q2d5sr9feqrm0",
    "bitcoincash:qz0vjl5vv6wkgljwm4fl3rexalpaxyzyfs42m22uy5",
    "bitcoincash:qr9kc0xe6gfza2up8c8kxad3w045uk7lavjf0jwflt",
    "bitcoincash:qpkddmmd6vg50kc83j2aen8fphyjvglc8cjvlm4auz",
    "bitcoincash:qzp6yqxue4xspg4l6d0uvhtfdcpp39japghqe43hg9",
    "bitcoincash:qr2jy9n8yqzj7tsthwrw6ej9ur5tpfhxsu3hk4n3xz",
    "bitcoincash:qqtl342ulttc9f9dw7gsyv40w4ju8cpass9fmaqxda",
    "bitcoincash:qrg9yysdvvs2l3mdyt4fgcxphtejg90tsyzpstvp33",
    "bitcoincash:qq94ktuql2dy8xt2vexqx6c988hrm07cjys4u2yqzt",
    "bitcoincash:qp2fvlgs6f5zs9luasjkgepxxfaa57l5qgsadmev8c"
};

var balances = client.GetWalletBalances(addresses);

Display("balances", balances);

Console.ReadLine();

static void Display(string name, object obj)
{
    Console.WriteLine();
    Console.WriteLine($"{name}:");
    Console.WriteLine();
    Console.WriteLine(JsonConvert.SerializeObject(obj, Formatting.Indented));
    Console.WriteLine();
}
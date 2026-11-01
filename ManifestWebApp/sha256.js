/* Incremental SHA-256 (FIPS 180-4), standalone, no external dependencies. */
/*
 * SHA-256 implementation according to FIPS 180-4
 *
 * Algorithm specified by NIST (National Institute of Standards and Technology).
 *
 * JavaScript implementation generated with ChatGPT (OpenAI).
 *
 * No external JavaScript libraries required.
 */
'use strict';
class Sha256 {
  constructor() {
    this.h = new Uint32Array([0x6a09e667,0xbb67ae85,0x3c6ef372,0xa54ff53a,0x510e527f,0x9b05688c,0x1f83d9ab,0x5be0cd19]);
    this.buffer = new Uint8Array(64); this.used = 0; this.bytes = 0;
  }
  static get k() { return Sha256._k || (Sha256._k = new Uint32Array([
    0x428a2f98,0x71374491,0xb5c0fbcf,0xe9b5dba5,0x3956c25b,0x59f111f1,0x923f82a4,0xab1c5ed5,
    0xd807aa98,0x12835b01,0x243185be,0x550c7dc3,0x72be5d74,0x80deb1fe,0x9bdc06a7,0xc19bf174,
    0xe49b69c1,0xefbe4786,0x0fc19dc6,0x240ca1cc,0x2de92c6f,0x4a7484aa,0x5cb0a9dc,0x76f988da,
    0x983e5152,0xa831c66d,0xb00327c8,0xbf597fc7,0xc6e00bf3,0xd5a79147,0x06ca6351,0x14292967,
    0x27b70a85,0x2e1b2138,0x4d2c6dfc,0x53380d13,0x650a7354,0x766a0abb,0x81c2c92e,0x92722c85,
    0xa2bfe8a1,0xa81a664b,0xc24b8b70,0xc76c51a3,0xd192e819,0xd6990624,0xf40e3585,0x106aa070,
    0x19a4c116,0x1e376c08,0x2748774c,0x34b0bcb5,0x391c0cb3,0x4ed8aa4a,0x5b9cca4f,0x682e6ff3,
    0x748f82ee,0x78a5636f,0x84c87814,0x8cc70208,0x90befffa,0xa4506ceb,0xbef9a3f7,0xc67178f2])); }
  static rotr(x,n) { return (x>>>n)|(x<<(32-n)); }
  block(b, offset=0) {
    const w = new Uint32Array(64);
    for(let i=0;i<16;i++) { let j=offset+i*4; w[i]=((b[j]<<24)|(b[j+1]<<16)|(b[j+2]<<8)|b[j+3])>>>0; }
    for(let i=16;i<64;i++) {
      const x=w[i-15], y=w[i-2];
      w[i]=(w[i-16]+(Sha256.rotr(x,7)^Sha256.rotr(x,18)^(x>>>3))+w[i-7]+(Sha256.rotr(y,17)^Sha256.rotr(y,19)^(y>>>10)))>>>0;
    }
    let [a,b0,c,d,e,f,g,h]=this.h, k=Sha256.k;
    for(let i=0;i<64;i++) {
      const t1=(h+(Sha256.rotr(e,6)^Sha256.rotr(e,11)^Sha256.rotr(e,25))+((e&f)^(~e&g))+k[i]+w[i])>>>0;
      const t2=((Sha256.rotr(a,2)^Sha256.rotr(a,13)^Sha256.rotr(a,22))+((a&b0)^(a&c)^(b0&c)))>>>0;
      h=g;g=f;f=e;e=(d+t1)>>>0;d=c;c=b0;b0=a;a=(t1+t2)>>>0;
    }
    const z=[a,b0,c,d,e,f,g,h]; for(let i=0;i<8;i++) this.h[i]=(this.h[i]+z[i])>>>0;
  }
  update(input) {
    const data = input instanceof Uint8Array ? input : new Uint8Array(input);
    this.bytes+=data.length;
    let i=0;
    if(this.used) { const n=Math.min(64-this.used,data.length);this.buffer.set(data.subarray(0,n),this.used);this.used+=n;i+=n;if(this.used===64){this.block(this.buffer);this.used=0;} }
    while(i+64<=data.length) { this.block(data,i); i+=64; }
    if(i<data.length) { this.buffer.set(data.subarray(i),0);this.used=data.length-i; }
    return this;
  }
  digestHex() {
    const bits=this.bytes*8;
    this.buffer[this.used++]=0x80;
    if(this.used>56){this.buffer.fill(0,this.used,64);this.block(this.buffer);this.used=0;}
    this.buffer.fill(0,this.used,56);
    const hi=Math.floor(bits/0x100000000),lo=bits>>>0;
    for(let i=0;i<4;i++){this.buffer[56+i]=(hi>>>(24-i*8))&255;this.buffer[60+i]=(lo>>>(24-i*8))&255;}
    this.block(this.buffer);
    return Array.from(this.h,x=>x.toString(16).padStart(8,'0')).join('');
  }
}
async function hashFile(file, progress) {
  const sha=new Sha256(), chunkSize=4*1024*1024;
  for(let offset=0;offset<file.size;offset+=chunkSize){
    const buf=await file.slice(offset,Math.min(offset+chunkSize,file.size)).arrayBuffer();
    sha.update(new Uint8Array(buf));
    if(progress)progress(Math.min(offset+chunkSize,file.size));
  }
  return sha.digestHex();
}

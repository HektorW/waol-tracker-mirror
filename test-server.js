/* jshint node:true, esnext:true */
'use strict';

const net = require('net');

let socket = new net.Socket();

socket.setEncoding('utf8');

socket.connect({
	host: 'localhost',
	port: 8004
});

socket.on('data', (data) => { console.log('Server: ', data); });

socket.on('connect', () => {
	console.log('Connected to socket');

	setTimeout(() => {
		console.log('Writing data');
		socket.write(JSON.stringify({
			distance: 1.0,
			color: {
				r: 255,
				g: 0,
				b: 0
			},
			info: {
				name: 'Name',
				surname: 'Surname',
				email: 'Email',
				height: 'Height',
				shoeSize: 'ShoeSize'
			}
		}), 'utf8');
	}, 1000);
});


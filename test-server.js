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

	setInterval(() => {
		console.log('Writing data');
		socket.write(JSON.stringify({
			distance: 1.0,
			color: {
				r: 133,
				g: 20,
				b: 75
			},
			info: {
				name: 'Hektor',
				surname: 'Wallin',
				email: 'hektorw@gmail.com',
				height: '182',
				shoeSize: '41'
			}
		}), 'utf8');
	}, 1000);
});


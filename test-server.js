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

function randInt(min, max) { return Math.floor(min + (Math.random() * (max - min))); }

socket.on('connect', () => {
	console.log('Connected to socket');

	let color = {
		r: 133,
		g: 20,
		b: 75
	};

	setInterval(() => {
		console.log('Changing color');
		color.r = randInt(0, 255);
		color.g = randInt(0, 255);
		color.b = randInt(0, 255);
	}, 5000);

	setInterval(() => {
		socket.write(JSON.stringify({
			distance: 1.0,
			color,
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


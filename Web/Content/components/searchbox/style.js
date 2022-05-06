import styled from 'styled-components';

export const Input = styled.input`
	width: 222px;
	height: 3em;
	padding: 10px;
	background: #f3f3f3;
	box-shadow: inset 0px 4px 4px rgba(0, 0, 0, 0.1);
	border-radius: 0.5em;
	border: none;
`;

export const Ul = styled.ul`
	display: contents;
`;

export const Li = styled.ul`
	width: 222px;
    font-weight: bold;
    height: 51px;
    padding: 0.5em 1em 0.5em 1em;
    margin: 0px;
    display: block;
    overflow: hidden;
    white-space: nowrap;
    background: #2d2d2d;
    border-bottom: 1px solid white;
	&:hover {
		cursor: pointer;
		background-color: #3a3a3a;
	}
`;

export const Span = styled.span`
    display: block;
    overflow: hidden;
    position: absolute;
    white-space: nowrap;
    transform: translateX(0);
    transition: 1s;
    &:hover {
        cursor: pointer;
        width: auto;
        transform: translateX(calc(200px - 100%));
    }
`;

export const SuggestContainer = styled.div`
	height: 240px;
    position: absolute;
    overflow: scroll;
    border-radius: 0.5em;
    cursor: pointer;
    box-shadow: inset 0px 4px 4px rgba(0, 0, 0, 0.1);
	&::-webkit-scrollbar {
		display: none;
	}
	-ms-overflow-style: none; /* IE and Edge */
	scrollbar-width: none; /* Firefox */
`;
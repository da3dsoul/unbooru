import axios from "axios";
import React, { useState, useCallback } from 'react';
import {Input, Ul, Li, SuggestContainer, Span} from './style';
import debounce from 'lodash.debounce';
import regeneratorRuntime from "regenerator-runtime";

const searchTags = ['tag:', 'tagID:', 'pixivID:', 'artist:', 'artistID:', 'aspect:', 'filesize:'];
const tagUrl = axios.create({baseURL: '/api/Tag/ByName/'});

const getApiSuggestions = (word) => {
    return tagUrl
        .get(`/${word}`)
        .then((response) => {
            return response.data;
        })
        .catch((error) => {
            return error;
        });
};

export default function SearchInput({ placeholder, }) {
    const [inputValue, setInputValue] = useState('');
    const [options, setOptions] = useState([]);
    const [loading, setLoading] = useState(false);

    const getApiUrl = (url) => {
        window.open(url, '_blank');
    };

    const getSuggestions = async (word) => {
        if (word) {
            setLoading(true);
            let response = await getApiSuggestions(word);
            setOptions(response);
            setLoading(false);
        } else {
            setOptions([]);
        }
    };

    const debouncedSave = useCallback(debounce((newValue) => getSuggestions(newValue), 1000), []);

    const updateValue = (newValue) => {
        setInputValue(newValue);
        debouncedSave(newValue);
    };

    return (
        <div>
            <Input value={inputValue} onChange={(input) => updateValue(input.target.value)} placeholder={placeholder} />
            <SuggestContainer>
                <Ul>
                    {loading && <Li>Loading...</Li>}
                    {options?.length > 0 &&
                        !loading &&
                        options?.map((value, index) => (
                            <Li key={`${value.name}-${index}`} onClick={() => getApiUrl(`/Search?tagID=${value.imageTagId}`)}>
                                <Span>{value.name}</Span>
                            </Li>
                        ))}
                </Ul>
            </SuggestContainer>
        </div>
    );
}
<?php

if (!defined('BASEPATH'))
    exit('No direct script access allowed');

class Music extends CI_Controller {

    public function __construct() {
        parent::__construct();
        #$this->output->enable_profiler(TRUE);
        $this->config->load('base');
        $this->load->model(array('musics'));
        $this->load->library(array('pagination', 'form_validation'));

        $this->logined->logincheck();
    }

    public function index() {
        $search = '';
        $data['per_page'] = '';
        if (!empty($_GET)) {
            $search = $this->input->get();
            $data['search'] = $search;
            $data['per_page'] = !empty($search['per_page']) ? $search['per_page'] : '';
        }

        $data['list'] = $this->musics->get_list($search, $data['per_page']);

        //ページング
        //件数を取得
        $data['count_row'] = $this->musics->get_count_row($search);
        $index = 0;
        $query = '';
        if (!empty($search)) {
            foreach ($search as $k => $v) {
                if (is_array($v)) {
                    foreach ($v as $v2) {
                        $query .= '&' . ($k . '[]') . '=' . ($v2);
                    }
                } else {
                    $query .= '&' . ($k) . '=' . ($v);
                }
                $index++;
            }
            $query = substr_replace($query, '?', 0, 1);
        }
        $url = base_url() . 'index.php/music';
        if (!empty($query)) {
            $url .= $query;
        }
        $data['paging'] = '';
        $data['paging'] = $this->pagination($url, $data['count_row']);
        $data['start_row'] = ($data['count_row'] === 0) ? 0 : $data['per_page'] + 1;
        $data['end_row'] = ($data['count_row'] === 0) ? 0 : $data['per_page'] + count($data['list']);
        $this->load->view('music/index', $data);
    }

    public function input($id = '') {
        if (!empty($id)) {
            //編集
            $data['method'] = '編集';
            $data['id'] = $id;
            $data['data'] = $this->musics->get_music($id);
        } else {
            //登録
            $data['method'] = '登録';
        }
        $this->load->view('music/input', $data);
    }

    public function conf($id = '') {

        if (empty($_POST)) {
            show_404();
            exit();
        }
        $data['post'] = $this->input->post();
        if (!empty($id)) {
            //編集
            $data['method'] = '編集';
            $data['id'] = $id;
        } else {
            //登録
            $data['method'] = '登録';
        }
        $this->set_validation_rules();
        if ($this->form_validation->run() === false) {
            $this->load->view('music/input', $data);
        } else {
            $this->load->view('music/conf', $data);
        }
    }

    public function comp_temp($id = '') {
        if (empty($_POST)) {
            show_404();
            exit();
        }
        $this->set_validation_rules();
        if ($this->form_validation->run() === false) {
            show_404();
            exit();
        }
        if (!empty($id)) {
            //編集
            $this->musics->update_music($id, $this->input->post());
        } else {
            //登録
            $this->musics->insert_music($this->input->post());
        }
        //$this->comp();
        redirect('music/comp/'.$id);
    }

    public function comp($id = '') {
        if (!empty($id)) {
            //編集
            $data['method'] = '編集';
        } else {
            //登録
            $data['method'] = '登録';
        }
        $this->load->view('music/comp', $data);
    }

    public function delete_temp($id = '') {
        if (empty($id)) {
            show_404();
            exit();
        }
        $this->musics->delete_music($id);
        redirect('music/delete/' . $id);
    }

    public function delete($id = '') {
        if (empty($id)) {
            show_404();
            exit();
        }
        $this->load->view('music/delete_comp');
    }

    /**
     * ページング設定
     * @param type $url
     * @param type $total_rows
     */
    private function pagination($url = '', $total_rows = '') {

        $config['first_link'] = '最初';
        $config['last_link'] = '最後';
        $config['page_query_string'] = TRUE;
        $config['base_url'] = $url;
        $config['total_rows'] = $total_rows;
        $config['per_page'] = $this->config->item('disp_num');

        $this->pagination->initialize($config);

        return $this->pagination->create_links();
    }

    public function set_validation_rules() {
        $this->form_validation->set_rules('song_title', '曲名', 'required');
        $this->form_validation->set_rules('lyricist', '作詞', 'required');
        $this->form_validation->set_rules('composer', '作曲', 'required');
        $this->form_validation->set_rules('singer', '歌手', 'required');
        $this->form_validation->set_rules('genre', 'ジャンル', 'required');
        #$this->form_validation->set_rules('song_time', '曲の長さ', 'required');
        $this->form_validation->set_rules('release_date', 'リリース日', 'required');

        $this->form_validation->set_error_delimiters('<p class="error">※', '</p>');
    }

}

<?php

if (!defined('BASEPATH'))
    exit('No direct script access allowed');

class Boxes extends CI_Model {


    public function __construct() {
        parent::__construct();
        $this->load->database();
    }


     /**
      * 受け取ったIDの端末情報を返す
      */
    public function get_box($id) {
        $this->db->select('id');
        $this->db->select('entry_date');
        $this->db->select('X(point) as x');
        $this->db->select('Y(point) as y');
        $this->db->select('delete_date');
        $this->db->select('prefectures');
        $this->db->from('boxes');
        $this->db->where('id', $id);
        
        $query = $this->db->get();
        
        return $query->row();
    }

    

    /**
     * 端末一覧を返す
     */
    function get_list($search = '', $offset = ''){
        $this->db->select('id');
        $this->db->select('entry_date');
        $this->db->select('X(point) as x');
        $this->db->select('Y(point) as y');
        $this->db->select('delete_date');
        $this->db->select('prefectures');
        $this->db->from('boxes');
        $this->db->where($this->create_where($search));
        
        $query = $this->db->get();
        
        return $query->result();
    }


    /**
     * 受け取った配列からwhere句を作成し、返す
     */
    private function create_where($search) {
        $where = array();
        if (!empty($search['pref'])) {
            if ($search['pref'] != '指定しない') {
                $where += array('prefectures =' => $search['pref']);
            }
        }
        if (!empty($search['entry_date_over'])) {
            $where += array('entry_date >=' => $search['entry_date_over']);
        }
        if (!empty($search['entry_date_under'])) {
            $where += array('entry_date <=' => $search['entry_date_under']);
        }

        return $where;
    }


    /**
     *
     */
    public function add($newBox) {
        $points = $this->getPointsFromAddress($newBox['address']);

        $sql = "insert into boxes (point, prefectures, address) ";
        $sql .= "values(geomFromText('point(". $points['x']. " ". $points['y']. ")'), ?, ?)";
        $this->db->query($sql, array($newBox['prefectures'], $newBox['address']));
    }


    /**
     * Google Maps APIからJSONオブジェクトを取得する
     */
    private function getMaps($parameter) {
        // APIリクエストURLの作成
        $url = 'http://maps.googleapis.com/maps/api/geocode/json?language=ja&sensor=true_or_false';
        if (array_key_exists('latlng', $parameter)) {
            $url .= '&latlng='. $parameter['latlng'];
        }
        if (array_key_exists('address', $parameter)) {
            $url .= '&address='. $parameter['address'];
        }

        // JSONの取得
        $json = "";
        $cp = curl_init();
        curl_setopt($cp, CURLOPT_RETURNTRANSFER, 1);
        curl_setopt($cp, CURLOPT_URL, $url);
        curl_setopt($cp, CURLOPT_TIMEOUT, 60);
        $json = curl_exec($cp);
        curl_close($cp);

        // JSONを配列へデコード
        $obj = json_decode($json);

        return $obj;
    }


    /**
     * 住所から緯度経度を取得する
     */
    private function getPointsFromAddress($address) {
        $parameter = array('address' => $address);
        $obj = $this->getMaps($parameter);

        $points = false;
        if ($obj['status'] == 'OK') {
            $points = array(
                'x' => $obj['result']['geometry']['location']['lng'],
                'y' => $obj['result']['geometry']['location']['lat']
            );
        }

        return $points;
    }


}